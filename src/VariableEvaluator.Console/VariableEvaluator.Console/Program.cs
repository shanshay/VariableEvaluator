using VariableEvaluator.Core.Services;

class Program
{
    static void Main(string[] args)
    {
        IVariableEvaluatorService evaluator = new ExpressionEvaluatorService();
        string[] testFiles = { "Data/case1.txt", "Data/case2.txt", "Data/case3.txt" };

        foreach (var file in testFiles)
        {
            Console.WriteLine($"=== Результаты для {Path.GetFileName(file)} ===");
            try
            {
                var results = evaluator.EvaluateFile(file);
                foreach (var kvp in results)
                {
                    Console.WriteLine($"{kvp.Key} = {kvp.Value}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обработке файла: {ex.Message}");
            }
            Console.WriteLine();
        }
    }
}