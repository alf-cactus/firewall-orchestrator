using FWO.Api.Client;
using FWO.Basics;
using FWO.Config.Api;
using FWO.NetworkTopology;
using FWO.Api.Client.Queries;
using FWO.Data;

namespace FWO.Services.PathAnalysis
{
    public class NetworkZoneTreePathAnalyzer(ApiConnection apiConnection, GlobalConfig globalConfig) : IPathAnalyzer
    {
        private sealed record MatrixData
        {
            public List<ComplianceNetworkZone> Zones { get; init; } = [];
            public List<NetworkZoneIpRange> IpRanges { get; init; } = [];
            public List<NetworkZoneDeviceIpRange> RootPaths { get; init; } = [];
            public List<NetworkZoneDeviceIpRange> InternetPaths { get; init; } = [];
        }
        public async Task<PathAnalysisResult> AnalyzeAsync(PathAnalysisRequest request)
        {
            long matrixId = request.MatrixId ?? globalConfig.DesignatedZoneMatrixId;
            //todo: if matrixId==0 warning in ui
            MatrixData networkData = await LoadNetworkDataAsync(matrixId);
            // 2. reinen Algorithmus rufen - lebt in FWO.NetworkTopology, kennt keine ApiConnection

            // 3. Ergebnis auf PathAnalysisResult mappen, Devices deduplizieren
        }

        private async Task<MatrixData> LoadNetworkDataAsync(long matrixId)
        {
            Task<List<ComplianceNetworkZone>> zonesTask = apiConnection.SendQueryAsync<List<ComplianceNetworkZone>>(
                NetworkZoneQueries.getNetworkZonesForMatrix, new { criterionId = matrixId });
            Task<List<NetworkZoneIpRange>> ipRangesTask = apiConnection.SendQueryAsync<List<NetworkZoneIpRange>>(
                NetworkZoneQueries.getIpRangesForMatrix, new { matrixId = matrixId });
            Task<List<NetworkZoneDeviceIpRange>> rootPathsTask  = apiConnection.SendQueryAsync<List<NetworkZoneDeviceIpRange>>(
                NetworkZoneQueries.getNetworkZoneDeviceIpRangeRoot, new { criterionId = matrixId });
            Task<List<NetworkZoneDeviceIpRange>> internetPathsTask  = apiConnection.SendQueryAsync<List<NetworkZoneDeviceIpRange>>(
                NetworkZoneQueries.getNetworkZoneDeviceIpRangeInternet, new { criterionId = matrixId });

            await Task.WhenAll(zonesTask, ipRangesTask, rootPathsTask, internetPathsTask);
            return new MatrixData
            {
                Zones = zonesTask.Result ?? [],
                IpRanges = ipRangesTask.Result ?? [],
                RootPaths = rootPathsTask.Result ?? [],
                InternetPaths = internetPathsTask.Result ?? []
            };
        }
    }
}