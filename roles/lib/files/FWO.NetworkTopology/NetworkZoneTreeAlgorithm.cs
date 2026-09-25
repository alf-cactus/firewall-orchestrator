using FWO.Data;
using FWO.Basics;
using NetTools;
using System.Net;

namespace FWO.NetworkTopology
{
    public sealed record MatrixData
    {
        public List<ComplianceNetworkZone> Zones { get; init; } = [];
        public List<NetworkZoneIpRange> IpRanges { get; init; } = [];
        public List<NetworkZoneDeviceIpRange> RootPaths { get; init; } = [];
        public List<NetworkZoneDeviceIpRange> InternetPaths { get; init; } = [];
    }
    /// <summary>One path query against the matrix this algorithm was built for.</summary>
    public sealed record QueryInput
    {
        public List<IPAddressRange> Sources { get; init; } = [];
        public List<IPAddressRange> Destinations { get; init; } = [];
    }
    /// <summary>One firewall device found on a path.</summary>
    public sealed record PathDevice
    {
        /// <summary>Device id as stored in device.dev_id.</summary>
        public int Id { get; init; }

        /// <summary>Device name as stored in device.dev_name.</summary>
        public string Name { get; init; } = "";
    }
    /// <summary>Information about zones in PathSegment.</summary>
    public enum ZoneKind
    {
        /// <summary>A zone configured in the matrix.</summary>
        Configured,
        /// <summary>The auto calculated internet zone.</summary>
        Internet,
        /// <summary>The auto calculated catch-all zone for internal addresses no zone covers.</summary>
        UndefinedInternal
    }
    /// <summary>Source or destination of a path.</summary>
    public sealed record PathEndpoint
    {
        public int IpRangeId { get; init; }
        public int ZoneId { get; init; }
        public ZoneKind ZoneKind { get; init; }
    }
    public sealed record PathSegment
    {
        public PathEndpoint Source { get; init; } = new();
        public PathEndpoint Destination { get; init; } = new();
        public List<PathDevice> Devices { get; init; } = [];
    }
    public class NetworkZoneTreeAlgorithm
    {
        /// <summary>One ip range of the matrix together with its parsed address range.</summary>
        private sealed record ParsedIpRange(NetworkZoneIpRange Row, IPAddressRange Range);
        private readonly MatrixData matrixData;
        private readonly List<ParsedIpRange> parsedIpRanges = [];
        private readonly int? internetZoneId;
        private readonly int? undefinedInternalZoneId;
        private readonly Dictionary<int, List<PathDevice>> rootPathByIpRangeId;
        private readonly Dictionary<int, List<PathDevice>> internetPathByIpRangeId;

        public NetworkZoneTreeAlgorithm(MatrixData matrixData)
        {
            this.matrixData = matrixData;
            internetZoneId = matrixData.Zones.FirstOrDefault(zone => zone.IsAutoCalculatedInternetZone)?.Id;
            undefinedInternalZoneId = matrixData.Zones.FirstOrDefault(zone => zone.IsAutoCalculatedUndefinedInternalZone)?.Id;

            //ComplianceNetworkZone? internetZone = matrixData.Zones.FirstOrDefault(zone => zone.IsAutoCalculatedInternetZone);
            //ComplianceNetworkZone? undefinedInternalZone = matrixData.Zones.FirstOrDefault(zone => zone.IsAutoCalculatedUndefinedInternalZone);

            foreach (NetworkZoneIpRange zoneRange in matrixData.IpRanges)
            {
                parsedIpRanges.Add(new ParsedIpRange(zoneRange, ToRange(zoneRange)));
            }

            rootPathByIpRangeId = [];
            foreach (NetworkZoneDeviceIpRange row in matrixData.RootPaths.OrderBy(row => row.OrderToRoot))
            {
                if (!rootPathByIpRangeId.TryGetValue(row.IpRangeId, out List<PathDevice>? devices))
                {
                    devices = [];
                    rootPathByIpRangeId[row.IpRangeId] = devices;
                }
                devices.Add(new PathDevice { Id = row.DeviceId, Name = row.Device?.Name ?? "" });
            }

            internetPathByIpRangeId = [];
            foreach (NetworkZoneDeviceIpRange row in matrixData.InternetPaths.OrderBy(row => row.OrderToInternet))
            {
                if (!internetPathByIpRangeId.TryGetValue(row.IpRangeId, out List<PathDevice>? devices))
                {
                    devices = [];
                    internetPathByIpRangeId[row.IpRangeId] = devices;
                }
                devices.Add(new PathDevice { Id = row.DeviceId, Name = row.Device?.Name ?? "" });
            }
        }

        public List<PathSegment> FindDeviceInPath(QueryInput input)
        {
            List<NetworkZoneIpRange> sourceRanges = LookupRelevantRanges(input.Sources);
            List<NetworkZoneIpRange> destinationRanges = LookupRelevantRanges(input.Destinations);
            List <PathSegment> CalculatePaths(sourceRanges, destinationRanges);
        }

        private List<NetworkZoneIpRange> LookupRelevantRanges(List<IPAddressRange> inputRanges)
        {
            List<NetworkZoneIpRange> relevantRanges = [];
            foreach (ParsedIpRange zoneRange in parsedIpRanges)
            {
                foreach (IPAddressRange inputRange in inputRanges)
                {
                    if (IpOperations.RangeOverlapExists(zoneRange.Range, inputRange))
                    {
                        relevantRanges.Add(zoneRange.Row);
                        break;
                    }
                }
            }
            return relevantRanges;
        }

        private List <PathSegment> CalculatePaths(
            List<NetworkZoneIpRange> sourceRanges, List<NetworkZoneIpRange> destinationRanges)
        {
            List<PathSegment> segments = [];
            List<PathEndpoint> destinationEndpoints = [.. destinationRanges.Select(ToEndpoint)];
            List<PathEndpoint> sourceEndpoints = [.. sourceRanges.Select(ToEndpoint)];
            
            foreach (PathEndpoint sourceEndpoint in sourceEndpoints)
            {
                foreach (PathEndpoint destinationEndpoint in destinationEndpoints)
                {
                    segments.Add(new PathSegment
                    {
                        Source = sourceEndpoint,
                        Destination = destinationEndpoint,
                        Devices = FindDevices(sourceEndpoint, destinationEndpoint)
                    });
                }
            }
            return segments;
        }
        
        private PathEndpoint ToEndpoint(NetworkZoneIpRange range)
        {
            return new PathEndpoint
            {
                IpRangeId = range.Id,
                ZoneId = range.NetworkZoneId,
                ZoneKind = ClassifyZone(range.NetworkZoneId)
            };
        }

        private ZoneKind ClassifyZone(int zoneId)
        {
            if (zoneId == internetZoneId) return ZoneKind.Internet;
            if (zoneId == undefinedInternalZoneId) return ZoneKind.UndefinedInternal;
            return ZoneKind.Configured;
        }

        private List<PathDevice> FindDevices(PathEndpoint sourceEndpoint, PathEndpoint destinationEndpoint)
        {
            if (sourceEndpoint.ZoneKind == ZoneKind.UndefinedInternal || destinationEndpoint.ZoneKind == ZoneKind.UndefinedInternal)
            {
                return [];
            }
            else if (sourceEndpoint.ZoneKind == ZoneKind.Internet && destinationEndpoint.ZoneKind == ZoneKind.Internet)
            {
                return [];
            }
            else if (sourceEndpoint.ZoneKind == ZoneKind.Internet)
            {
                return internetPathByIpRangeId[destinationEndpoint.IpRangeId];
            }
            else if (destinationEndpoint.ZoneKind == ZoneKind.Internet)
            {
                return internetPathByIpRangeId[sourceEndpoint.IpRangeId];
            }
            //hier weiter mitkürzung
        }

        private static IPAddressRange ToRange(NetworkZoneIpRange ipRange) =>
            new(IPAddressRange.Parse(ipRange.IpRangeStart).Begin,
                IPAddressRange.Parse(ipRange.IpRangeEnd).Begin);
    }
}