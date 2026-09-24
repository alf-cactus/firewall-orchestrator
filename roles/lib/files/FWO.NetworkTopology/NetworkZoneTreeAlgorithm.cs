using FWO.Data;
using FWO.Basics;
using NetTools;

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
    public sealed record ZonePathSegment
    {
        public int SourceZoneId { get; init; }
        public int DestinationZoneId { get; init; }
        public List<PathDevice> Devices { get; init; } = [];
    }
    public class NetworkZoneTreeAlgorithm
    {
        /// <summary>One ip range of the matrix together with its parsed address range.</summary>
        private sealed record ParsedIpRange(NetworkZoneIpRange Row, IPAddressRange Range);
        private readonly MatrixData matrixData;
        private readonly List<ParsedIpRange> parsedIpRanges = [];
        public NetworkZoneTreeAlgorithm(MatrixData matrixData)
        {
            this.matrixData = matrixData;
            foreach (NetworkZoneIpRange zoneRange in matrixData.IpRanges)
            {
                parsedIpRanges.Add(new ParsedIpRange(zoneRange, ToRange(zoneRange)));
            }
        }

        public List<ZonePathSegment> FindDeviceInPath(QueryInput input)
        {
            List<NetworkZoneIpRange> sourceRanges = LookupRelevantRanges(input.Sources);
            List<NetworkZoneIpRange> destinationRanges = LookupRelevantRanges(input.Destinations);
            List <ZonePathSegment> CalculatePaths(sourceRanges, destinationRanges);
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

        private List <ZonePathSegment> CalculatePaths(
            List<NetworkZoneIpRange> sourceRanges, List<NetworkZoneIpRange> destinationRanges)
        {
            foreach (NetworkZoneIpRange sourceRange in sourceRanges)
            {
                foreach (NetworkZoneIpRange destinationRange in destinationRanges)
                {
                    
                }
            }
        }
        

        private static IPAddressRange ToRange(NetworkZoneIpRange ipRange) =>
            new(IPAddressRange.Parse(ipRange.IpRangeStart).Begin,
                IPAddressRange.Parse(ipRange.IpRangeEnd).Begin);
    }
}