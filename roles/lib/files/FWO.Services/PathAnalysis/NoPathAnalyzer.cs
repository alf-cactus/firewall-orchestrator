using FWO.Basics;

namespace FWO.Services.PathAnalysis
{
    public class NoPathAnalyzer : IPathAnalyzer
    {
        public Task<PathAnalysisResult> AnalyzeAsync(PathAnalysisRequest request)
        {
            return Task.FromResult(new PathAnalysisResult
            {
                AlgorithmId = GlobalConst.kPathAnalysisAlgorithmNone
            });
        }
    }
}