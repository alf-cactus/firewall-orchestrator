

namespace FWO.Services.PathAnalysis
{
    public interface IPathAnalyzer
    {
        public sealed class PathAnalysisRequest
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
        public sealed class PathAnalysisResult
        {
            /// <summary>Id of Algorithm in path_analysis_algorithm.</summary>
            public long AlgorithmId { get; init; }
            /// <summary>Deduplicated and unsorted list of devices in path.</summary>
            public List<PathDevice> Devices { get; init; } = [];
            /// <summary>List of input ranges that are not covered by network zones.
            /// Is empty if autoCalculateUndefinedInternalZone is enabled.</summary>
            public List<IPAddressRange> UnresolvedRanges { get; init; } = [];
        }
    }
}
