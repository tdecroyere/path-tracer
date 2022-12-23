dotnet run -c Release
dotnet run --project ../external/ResultsComparer matrix --input ./BenchmarkDotNet.Artifacts/ --base base --diff results -t 3%