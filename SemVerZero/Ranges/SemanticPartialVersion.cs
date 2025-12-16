using System.Text.RegularExpressions;

namespace SemVerZero.Ranges;

internal readonly partial struct SemanticPartialVersion {
    public long? Major { get; }
    public long? Minor { get; }
    public long? Patch { get; }
    public string? Prerelease { get; }

    [GeneratedRegex(@"^[v=\s]*(\d+|[Xx\*])(\.(\d+|[Xx\*])(\.(\d+|[Xx\*])(\-?([0-9A-Za-z\-\.]+))?(\+([0-9A-Za-z\-\.]+))?)?)?$")]
    private static partial Regex PartialVersionRegex();

    public SemanticPartialVersion(long? Major, long? Minor, long? Patch, string? Prerelease) {
        this.Major = Major;
        this.Minor = Minor;
        this.Patch = Patch;
        this.Prerelease = Prerelease;
    }
    public static SemanticPartialVersion Parse(scoped ReadOnlySpan<char> Input) {
        long? Major = null;
        long? Minor = null;
        long? Patch = null;
        string? Prerelease = null;

        Match Match = PartialVersionRegex().Match(Input.ToString());
        if (!Match.Success) {
            throw new ArgumentException($"Invalid version string: \"{Input}\"");
        }

        if (Match.Groups[1].Value is "X" or "x" or "*") {
            Major = null;
        }
        else {
            Major = long.Parse(Match.Groups[1].Value);
        }

        if (Match.Groups[2].Success) {
            if (Match.Groups[3].Value is "X" or "x" or "*") {
                Minor = null;
            }
            else {
                Minor = long.Parse(Match.Groups[3].Value);
            }
        }

        if (Match.Groups[4].Success) {
            if (Match.Groups[5].Value is "X" or "x" or "*") {
                Patch = null;
            }
            else {
                Patch = long.Parse(Match.Groups[5].Value);
            }
        }

        if (Match.Groups[6].Success) {
            Prerelease = Match.Groups[7].Value;
        }

        return new SemanticPartialVersion(Major, Minor, Patch, Prerelease);
    }
}