using FWO.Config.Api;
using FWO.Api.Client;
using FWO.Basics;
using FWO.Logging;

namespace FWO.Services.PathAnalysis
{
    public class PathAnalyzerFactory(ApiConnection apiConnection, GlobalConfig globalConfig)
    {
        public IPathAnalyzer Create()
        {
            switch (globalConfig.PathAnalysisAlgorithm)
            {
                case GlobalConst.kPathAnalysisAlgorithmNone:
                    return new NoPathAnalyzer();
                case GlobalConst.kPathAnalysisAlgorithmNetworkZoneTree:
                    return new NetworkZoneTreePathAnalyzer(apiConnection, globalConfig);
                default:
                    Log.WriteWarning("Path Analysis",
                        $"unknown path analysis algorithm id {globalConfig.PathAnalysisAlgorithm}, falling back to none");
                    return new NoPathAnalyzer();
            }
        }
    }
}
