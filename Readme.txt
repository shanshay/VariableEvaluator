Markdown
# Variable Expression Evaluator

A .NET (C#) console application designed to read files containing variables, evaluate their values ​​(handling arbitrary line ordering and nested dependencies), and verify the results via unit tests.

## Technologies Used
- **.NET 8.0**
- **NCalc** (a third-party library for parsing and evaluating mathematical expressions)
- **xUnit & FluentAssertions** (for unit testing)

## How to Run the Solution
1. Open a terminal in the root folder of the solution (`VariableEvaluator`).
2. Restore dependencies and build the project:
```bash
dotnet restore
dotnet build
