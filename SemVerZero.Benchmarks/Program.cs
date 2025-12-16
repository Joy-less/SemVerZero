using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

public static class Program {
    public static void Main() {
        BenchmarkSwitcher.FromAssemblies(AppDomain.CurrentDomain.GetAssemblies()).Run();
    }
}

[MemoryDiagnoser]
public class ParseBenchmarks {
    [Benchmark]
    public SemVerZero.SemanticVersion Parse_SemVerZero() {
        return SemVerZero.SemanticVersion.Parse("1.0.62-alpha.1");
    }
    [Benchmark]
    public SemanticVersioning.Version Parse_adamreeve() {
        return SemanticVersioning.Version.Parse("1.0.62-alpha.1");
    }
    [Benchmark]
    public Semver.SemVersion Parse_WalkerCodeRanger() {
        return Semver.SemVersion.Parse("1.0.62-alpha.1");
    }
}

[MemoryDiagnoser]
public class ToStringBenchmarks {
    [Benchmark]
    public string ToString_SemVerZero() {
        return new SemVerZero.SemanticVersion(1, 0, 62, "alpha.1").ToString();
    }
    [Benchmark]
    public string ToString_adamreeve() {
        return new SemanticVersioning.Version(1, 0, 62, "alpha.1").ToString();
    }
    [Benchmark]
    public string ToString_WalkerCodeRanger() {
        return new Semver.SemVersion(1, 0, 62, ["alpha", "1"]).ToString();
    }
}