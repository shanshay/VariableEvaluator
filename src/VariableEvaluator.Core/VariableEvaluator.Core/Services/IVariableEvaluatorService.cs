namespace VariableEvaluator.Core.Services
{
    public interface IVariableEvaluatorService
    {
        Dictionary<string, double> EvaluateFile(string filePath);
    }
}
