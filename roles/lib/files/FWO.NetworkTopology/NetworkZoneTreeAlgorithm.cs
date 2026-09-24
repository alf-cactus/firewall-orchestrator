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
    public class NetworkZoneTreeAlgorithm(MatrixData matrixData)
    {

        public List<ZonePathSegment> FindDeviceInPath(QueryInput input)
        {
            LookupInputZones(input.Sources);
        }

        private List<int> LookupInputZones(List<IPAddressRange> inputRanges)
        {
            List<int> zoneList = [];
            foreach (NetworkZoneIpRange zoneRange in matrixData.IpRanges)
            {
                foreach (IPAddressRange inputRange in inputRanges)
                {
                    if (IpOperations.RangeOverlapExists(ToRange(zoneRange), inputRange))
                    {
                        zoneList.Add(zoneRange.NetworkZoneId);
                        break;
                    }
                }
            }
            return zoneList;
        }

        private static IPAddressRange ToRange(NetworkZoneIpRange ipRange) =>
            new(IPAddressRange.Parse(ipRange.IpRangeStart).Begin,
                IPAddressRange.Parse(ipRange.IpRangeEnd).Begin);
    }
}