namespace GhostEye.Infrastructure.Benchmarks;

public sealed record BenchmarkRow(string Metric, string Before, string After, string Delta, bool IsImprovement);
