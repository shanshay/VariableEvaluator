using System;
using System.Collections.Generic;
using System.IO;
using NCalc;

namespace VariableEvaluator.Core.Services
{
    public class ExpressionEvaluatorService : IVariableEvaluatorService
    {
        public Dictionary<string, double> EvaluateFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            var rawLines = File.ReadAllLines(filePath);
            var expressions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var line in rawLines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split('=');
                if (parts.Length != 2) continue;

                string varName = parts[0].Trim();
                string expr = parts[1].Trim();
                expressions[varName] = expr;
            }

            var results = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            var evaluating = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            double EvaluateVariable(string varName)
            {
                if (results.TryGetValue(varName, out var cachedValue))
                    return cachedValue;

                if (!expressions.TryGetValue(varName, out var exprString))
                    throw new ArgumentException($"Variable '{varName}' was not found.");

                if (!evaluating.Add(varName))
                    throw new InvalidOperationException($"Circular dependency detected for variable '{varName}'.");

                exprString = ConvertMeanFunction(exprString);
                exprString = ConvertPowerOperator(exprString);

                var e = new NCalc.Expression(exprString);

                // NCalc сам запрашивает ровно те переменные, которые нужны формуле —
                // вычисляем их лениво и рекурсивно, порядок строк в файле не важен.
                e.EvaluateParameter += (name, args) =>
                {
                    if (expressions.ContainsKey(name))
                    {
                        args.Result = EvaluateVariable(name);
                    }
                };

                var computedObj = e.Evaluate();
                double finalValue = Convert.ToDouble(computedObj);

                evaluating.Remove(varName);
                results[varName] = finalValue;
                return finalValue;
            }

            foreach (var key in expressions.Keys)
            {
                EvaluateVariable(key);
            }

            return results;
        }

        // MEAN(a, b, c) → ((a + b + c) / 3.0). Внешние скобки обязательны:
        // иначе x / MEAN(a, b) превратилось бы в x / (a + b) / 2.0.
        private static string ConvertMeanFunction(string expression)
        {
            while (true)
            {
                int meanIndex = expression.IndexOf("MEAN(", StringComparison.OrdinalIgnoreCase);
                if (meanIndex == -1) break;

                int closeIndex = FindClosingBracket(expression, meanIndex + 4);

                string argsContent = expression.Substring(meanIndex + 5, closeIndex - (meanIndex + 5));
                var args = argsContent.Split(',');
                string replacement = $"(({string.Join(" + ", args)}) / {args.Length}.0)";

                expression = expression.Remove(meanIndex, (closeIndex + 1) - meanIndex).Insert(meanIndex, replacement);
            }

            return expression;
        }

        // В NCalc '^' — побитовый XOR, степень — функция Pow(x, y).
        // Разворачиваем справа налево, чтобы a^b^c дало Pow(a, Pow(b, c)).
        private static string ConvertPowerOperator(string expression)
        {
            while (true)
            {
                int caret = expression.LastIndexOf('^');
                if (caret == -1) break;

                int leftStart = FindOperandStart(expression, caret - 1);
                int rightEnd = FindOperandEnd(expression, caret + 1);

                string left = expression.Substring(leftStart, caret - leftStart).Trim();
                string right = expression.Substring(caret + 1, rightEnd - caret).Trim();

                expression = expression.Substring(0, leftStart)
                           + $"Pow({left}, {right})"
                           + expression.Substring(rightEnd + 1);
            }

            return expression;
        }

        private static bool IsOperandChar(char c) => char.IsLetterOrDigit(c) || c == '_' || c == '.';

        // Индекс первого символа операнда слева от '^' (pos — символ перед '^').
        private static int FindOperandStart(string s, int pos)
        {
            while (pos >= 0 && char.IsWhiteSpace(s[pos])) pos--;
            if (pos < 0)
                throw new ArgumentException($"Missing left operand for '^' in '{s}'.");

            if (s[pos] == ')')
            {
                int depth = 0;
                for (; pos >= 0; pos--)
                {
                    if (s[pos] == ')') depth++;
                    else if (s[pos] == '(' && --depth == 0) break;
                }
                if (pos < 0)
                    throw new ArgumentException($"Unbalanced brackets in '{s}'.");
            }

            // имя функции перед скобкой (Pow(..)^2) либо идентификатор/число
            int start = pos;
            while (start > 0 && IsOperandChar(s[start - 1])) start--;
            return start;
        }

        // Индекс последнего символа операнда справа от '^' (pos — символ после '^').
        private static int FindOperandEnd(string s, int pos)
        {
            while (pos < s.Length && char.IsWhiteSpace(s[pos])) pos++;
            if (pos < s.Length && (s[pos] == '-' || s[pos] == '+'))
            {
                pos++;
                while (pos < s.Length && char.IsWhiteSpace(s[pos])) pos++;
            }
            if (pos >= s.Length)
                throw new ArgumentException($"Missing right operand for '^' in '{s}'.");

            if (s[pos] == '(')
                return FindClosingBracket(s, pos);

            int end = pos;
            while (end + 1 < s.Length && IsOperandChar(s[end + 1])) end++;

            // вызов функции: name(...)
            int next = end + 1;
            while (next < s.Length && char.IsWhiteSpace(s[next])) next++;
            if (next < s.Length && s[next] == '(' && char.IsLetter(s[pos]))
                return FindClosingBracket(s, next);

            return end;
        }

        private static int FindClosingBracket(string s, int openIndex)
        {
            int depth = 0;
            for (int i = openIndex; i < s.Length; i++)
            {
                if (s[i] == '(') depth++;
                else if (s[i] == ')' && --depth == 0) return i;
            }
            throw new ArgumentException($"Unbalanced brackets in '{s}'.");
        }
    }
}
