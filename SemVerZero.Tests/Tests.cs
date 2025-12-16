using System.Text;

namespace SemVerZero.Tests;

public class Tests {
    [Fact]
    public void ReadmeTest() {
        SemanticVersion Version = SemanticVersion.Parse("1.0.0-rc.1");

        long Major = Version.Major;
        long Minor = Version.Minor;
        long Patch = Version.Patch;
        string? Prerelease = Version.Prerelease;
        string? Build = Version.Build;

        string String = Version.ToString();
        string MinimalString = Version.ToString(SemanticVersionFormat.Major);

        Major.ShouldBe(1);
        Minor.ShouldBe(0);
        Patch.ShouldBe(0);
        Prerelease.ShouldBe("rc.1");
        Build.ShouldBe(null);

        String.ShouldBe("1.0.0-rc.1");
        MinimalString.ShouldBe("1-rc.1");
    }

    [Fact]
    public void ConstructorTest() {
        SemanticVersion Version1 = new(1, 2, 3, "a", "b");
        Version1.ToString().ShouldBe("1.2.3-a+b");
        Version1.ToString(SemanticVersionFormat.Major).ShouldBe("1.2.3-a+b");

        SemanticVersion Version2 = new(1, 2, "a", "b");
        Version2.ToString().ShouldBe("1.2.0-a+b");
        Version2.ToString(SemanticVersionFormat.Major).ShouldBe("1.2-a+b");

        SemanticVersion Version3 = new(0, 5);
        Version3.ToString().ShouldBe("0.5.0");
        Version3.ToString(SemanticVersionFormat.Major).ShouldBe("0.5");

        SemanticVersion Version4 = new();
        Version4.ToString().ShouldBe("0.0.0");
        Version4.ToString(SemanticVersionFormat.Major).ShouldBe("0");
    }

    [Fact]
    public void InvalidConstructorTest() {
        Should.Throw<ArgumentOutOfRangeException>(() => {
            SemanticVersion Version1 = new(-1, 0, 0);
        });
        Should.Throw<FormatException>(() => {
            SemanticVersion Version2 = new(1, 0, 0, "");
        });
        Should.Throw<FormatException>(() => {
            SemanticVersion Version3 = new(1, 0, 0, Build: "  ");
        });
    }

    [Fact]
    public void ParseTest() {
        SemanticVersion Version1 = SemanticVersion.Parse("1.2.3");
        Version1.ToString().ShouldBe("1.2.3");

        SemanticVersion.TryParse("1.2.3", out SemanticVersion Version2).ShouldBeTrue();
        Version2.ToString().ShouldBe("1.2.3");
    }

    [Fact]
    public void TryFormatTest() {
        SemanticVersion Version1 = new(1, 24, 3);
        Span<char> Destination1 = stackalloc char[32];
        Version1.TryFormat(Destination1, out int CharsWritten1).ShouldBeTrue();
        Destination1[..CharsWritten1].ToString().ShouldBe("1.24.3");

        SemanticVersion Version2 = new(1, 24, 3);
        Span<byte> Destination2 = stackalloc byte[32];
        Version2.TryFormat(Destination2, out int CharsWritten2).ShouldBeTrue();
        Encoding.UTF8.GetString(Destination2[..CharsWritten2]).ShouldBe("1.24.3");
    }

    [Fact]
    public void OrderTest() {
        SemanticVersion[] Versions = [
            SemanticVersion.Parse("1.9.0"),
            SemanticVersion.Parse("1.10.0"),
            SemanticVersion.Parse("1.11.0"),
        ];

        for (int Index = 1; Index < Versions.Length; Index++) {
            SemanticVersion Left = Versions[Index - 1];
            SemanticVersion Right = Versions[Index];

            (Left < Right).ShouldBeTrue($"{Left} >= {Right}");
        }
    }

    [Fact]
    public void TagOrderTest() {
        SemanticVersion[] Versions = [
            SemanticVersion.Parse("1.0.0-alpha"),
            SemanticVersion.Parse("1.0.0-alpha.1"),
            SemanticVersion.Parse("1.0.0-alpha.beta"),
            SemanticVersion.Parse("1.0.0-beta"),
            SemanticVersion.Parse("1.0.0-beta.2"),
            SemanticVersion.Parse("1.0.0-beta.11"),
            SemanticVersion.Parse("1.0.0-rc.1"),
            SemanticVersion.Parse("1.0.0"),
        ];

        for (int Index = 1; Index < Versions.Length; Index++) {
            SemanticVersion Left = Versions[Index - 1];
            SemanticVersion Right = Versions[Index];

            (Left < Right).ShouldBeTrue($"{Left} >= {Right}");
        }
    }
}