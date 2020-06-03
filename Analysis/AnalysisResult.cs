namespace CoverageAnalysis
{
    public class AnalysisResult
    {
        public bool IsSuccess { get; }
        public int HandlerCount { get; }
        public int MissingCount { get; }
        public string AssemblyName { get; }

        public AnalysisResult(bool isSuccess, int handlerCount, int missingCount, string assemblyName)
        {
            this.IsSuccess = isSuccess;
            this.HandlerCount = handlerCount;
            this.MissingCount = missingCount;
            this.AssemblyName = assemblyName;
        }
    }
}
