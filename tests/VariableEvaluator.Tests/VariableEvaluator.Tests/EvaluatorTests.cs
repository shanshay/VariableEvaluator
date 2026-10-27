using System.IO;
using Xunit;
using FluentAssertions;
using VariableEvaluator.Core.Services;

namespace VariableEvaluator.Tests
{
    public class EvaluatorTests
    {
        private readonly IVariableEvaluatorService _evaluator = new ExpressionEvaluatorService();

        [Fact]
        public void Test_Case1()
        {
            // Берем файл из папки запуска тестов (куда он автоматически копируется)
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "case1.txt");

            var results = _evaluator.EvaluateFile(filePath);

            Math.Round(results["v_pi"], 2).Should().Be(3.14);
            Math.Round(results["r"], 2).Should().Be(4.00);
            Math.Round(results["circle_area"], 2).Should().Be(50.24);
            Math.Round(results["h"], 2).Should().Be(8.00);
            Math.Round(results["v"], 2).Should().Be(401.92);
            Math.Round(results["tsa"], 2).Should().Be(301.44);
        }

        [Fact]
        public void Test_Case2()
        {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "case2.txt");
            var results = _evaluator.EvaluateFile(filePath);

            Math.Round(results["m1"], 2).Should().Be(10.20);
            Math.Round(results["m2"], 2).Should().Be(9.80);
            Math.Round(results["mean_measurement"], 2).Should().Be(10.15);
            Math.Round(results["calibration_factor"], 2).Should().Be(1.15);
            Math.Round(results["calibrated_mean"], 2).Should().Be(11.67);
            Math.Round(results["offset"], 2).Should().Be(0.50);
            Math.Round(results["result"], 2).Should().Be(12.17);
        }

        [Fact]
        public void Test_Case3()
        {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "case3.txt");
            var results = _evaluator.EvaluateFile(filePath);

            Math.Round(results["average"], 2).Should().Be(1.45);
            Math.Round(results["efficiency"], 2).Should().Be(1.55);
            Math.Round(results["consistency"], 2).Should().Be(0.97);
            Math.Round(results["adjustment"], 2).Should().Be(180.02);
            Math.Round(results["result"], 2).Should().Be(330.04);
            Math.Round(results["quality_index"], 2).Should().Be(96.93);
            Math.Round(results["weighted_score"], 2).Should().Be(180.02);
            Math.Round(results["score"], 2).Should().Be(150.02);
        }
    }
}