using System.Text;

namespace SemVerZero.Tests;

public class Tests {
    [Fact]
    public void ConstructorTest() {
        SemanticVersion Version1 = new(1, 2, 3, "a", "b");
        Version1.ToString().ShouldBe("1.2.3-a+b");
        Version1.ToMinimalString().ShouldBe("1.2.3-a+b");

        SemanticVersion Version2 = new(1, 2, "a", "b");
        Version2.ToString().ShouldBe("1.2.0-a+b");
        Version2.ToMinimalString().ShouldBe("1.2-a+b");

        SemanticVersion Version3 = new(0, 5);
        Version3.ToString().ShouldBe("0.5.0");
        Version3.ToMinimalString().ShouldBe("0.5");

        SemanticVersion Version4 = new();
        Version4.ToString().ShouldBe("0.0.0");
        Version4.ToMinimalString().ShouldBe("0");
    }

    [Fact]
    public void InvalidConstructorTest() {
        Should.Throw<ArgumentOutOfRangeException>(() => {
            SemanticVersion Version1 = new(-1, 0, 0);
        });
        Should.Throw<ArgumentException>(() => {
            SemanticVersion Version2 = new(1, 0, 0, "");
        });
        Should.Throw<ArgumentException>(() => {
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
}