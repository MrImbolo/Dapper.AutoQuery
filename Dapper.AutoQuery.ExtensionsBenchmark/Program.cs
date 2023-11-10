using BenchmarkDotNet.Running;
using Dapper.AutoQuery.ExtensionsBenchmark;

BenchmarkRunner.Run<InsertBenchmark>();
BenchmarkRunner.Run<UpdateBenchmark>();
BenchmarkRunner.Run<SelectBenchmark>();
BenchmarkRunner.Run<DeleteBenchmark>();