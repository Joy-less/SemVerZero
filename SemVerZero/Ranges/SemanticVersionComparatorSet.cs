using System.Text.RegularExpressions;

namespace SemVerZero.Ranges;

public readonly struct SemanticVersionComparatorSet {
    public IEnumerable<SemanticVersionComparator>? Comparators { get; }

    public SemanticVersionComparatorSet() {
    }
    public SemanticVersionComparatorSet(params IEnumerable<SemanticVersionComparator>? Comparators) {
        this.Comparators = Comparators;
    }

    public bool IsMatch(SemanticVersion Version) {

    }

    public static SemanticVersionComparatorSet Parse(scoped ReadOnlySpan<char> Input) {
        List<SemanticVersionComparator> Comparators = [];

        Input = Input.Trim();

        int Index = 0;
        while (Index < Input.Length) {
            int StartIndex = Index;


        }

        return new SemanticVersionComparatorSet(Comparators);
    }
}

internal static partial class Desugarer {
    [GeneratedRegex(@"^\s*~\s*([0-9a-zA-Z\-\+\.\*]+)\s*")]
    private static partial Regex TildeRangeRegex();
    [GeneratedRegex(@"^\s*\^\s*([0-9a-zA-Z\-\+\.\*]+)\s*")]
    private static partial Regex CaretRangeRegex();
    [GeneratedRegex(@"^\s*([0-9a-zA-Z\-\+\.\*]+)\s+\-\s+([0-9a-zA-Z\-\+\.\*]+)\s*")]
    private static partial Regex HyphenRangeRegex();
    [GeneratedRegex(@"^\s*=?\s*([0-9a-zA-Z\-\+\.\*]+)\s*")]
    private static partial Regex StarRangeRegex();

    // Allows patch-level changes if a minor version is specified
    // on the comparator. Allows minor-level changes if not.
    public static (int, SemanticVersionComparator?, SemanticVersionComparator?)? TildeRange(string spec) {
        Match Match = TildeRangeRegex().Match(spec);
        if (!Match.Success) {
            return null;
        }

        SemanticVersion? minVersion = null;
        SemanticVersion? maxVersion = null;

        var version = SemanticPartialVersion.Parse(Match.Groups[1].Value);
        if (!version.Major.HasValue) {
            // ~x: any versions
            minVersion = version.ToZeroVersion();
        }
        else if (version.Minor.HasValue) {
            // Doesn't matter whether patch version is null or not,
            // the logic is the same, min patch version will be zero if null.
            minVersion = version.ToZeroVersion();
            maxVersion = new Version(version.Major.Value, version.Minor.Value + 1, 0);
        }
        else {
            minVersion = version.ToZeroVersion();
            maxVersion = new Version(version.Major.Value + 1, 0, 0);
        }

        return (
            Match.Length,
            new SemanticVersionComparator(SemanticVersionOperator.GreaterThanOrEqual, minVersion),
            new SemanticVersionComparator(SemanticVersionOperator.LessThanExcludingPrereleases, maxVersion)
        );
    }

    // Allows changes that do not modify the left-most non-zero digit
    // in the [major, minor, patch] tuple.
    public static (int, SemanticVersionComparator, SemanticVersionComparator)? CaretRange(string spec) {
        Match Match = CaretRangeRegex().Match(spec);
        if (!Match.Success) {
            return null;
        }

        SemanticVersion minVersion = null;
        SemanticVersion maxVersion = null;

        var version = SemanticPartialVersion.Parse(Match.Groups[1].Value);

        if (!version.Major.HasValue) {
            // ^x: any versions
            minVersion = version.ToZeroVersion();
        }
        else if (version.Major.Value > 0) {
            // Don't allow major version change
            minVersion = version.ToZeroVersion();
            maxVersion = new Version(version.Major.Value + 1, 0, 0);
        }
        else if (!version.Minor.HasValue) {
            // Don't allow major version change, even if it's zero
            minVersion = version.ToZeroVersion();
            maxVersion = new Version(version.Major.Value + 1, 0, 0);
        }
        else if (!version.Patch.HasValue) {
            // Don't allow minor version change, even if it's zero
            minVersion = version.ToZeroVersion();
            maxVersion = new Version(0, version.Minor.Value + 1, 0);
        }
        else if (version.Minor > 0) {
            // Don't allow minor version change
            minVersion = version.ToZeroVersion();
            maxVersion = new Version(0, version.Minor.Value + 1, 0);
        }
        else {
            // Only patch non-zero, don't allow patch change
            minVersion = version.ToZeroVersion();
            maxVersion = new Version(0, 0, version.Patch.Value + 1);
        }

        return (Match.Length,
            new SemanticVersionComparator(SemanticVersionOperator.GreaterThanOrEqual, minVersion),
            new SemanticVersionComparator(SemanticVersionOperator.LessThanExcludingPrereleases, maxVersion));
    }

    public static (int, SemanticVersionComparator, SemanticVersionComparator)? HyphenRange(string spec) {
        Match Match = HyphenRangeRegex().Match(spec);
        if (!Match.Success) {
            return null;
        }

        SemanticPartialVersion minPartialVersion = null;
        SemanticPartialVersion maxPartialVersion = null;

        // Parse versions from lower and upper ranges, which might
        // be partial versions.
        try {
            minPartialVersion = SemanticPartialVersion.Parse(Match.Groups[1].Value);
            maxPartialVersion = SemanticPartialVersion.Parse(Match.Groups[2].Value);
        }
        catch (ArgumentException) {
            return null;
        }

        // Lower range has any non-supplied values replaced with zero
        var minVersion = minPartialVersion.ToZeroVersion();

        Comparator.Operator maxOperator = maxPartialVersion.IsFull()
            ? Comparator.Operator.LessThanOrEqual : Comparator.Operator.LessThanExcludingPrereleases;

        SemanticVersion maxVersion = null;

        // Partial upper range means supplied version values can't change
        if (!maxPartialVersion.Major.HasValue) {
            // eg. upper range = "*", then maxVersion remains null
            // and there's only a minimum
        }
        else if (!maxPartialVersion.Minor.HasValue) {
            maxVersion = new Version(maxPartialVersion.Major.Value + 1, 0, 0);
        }
        else if (!maxPartialVersion.Patch.HasValue) {
            maxVersion = new Version(maxPartialVersion.Major.Value, maxPartialVersion.Minor.Value + 1, 0);
        }
        else {
            // Fully specified max version
            maxVersion = maxPartialVersion.ToZeroVersion();
        }
        return (Match.Length,
            new SemanticVersionComparator(SemanticVersionOperator.GreaterThanOrEqualIncludingPrereleases, minVersion),
            new SemanticVersionComparator(maxOperator, maxVersion));
    }

    public static (int, SemanticVersionComparator, SemanticVersionComparator)? StarRange(string spec) {
        Match Match = StarRangeRegex().Match(spec);
        if (!Match.Success) {
            return null;
        }

        SemanticPartialVersion version = null;
        try {
            version = SemanticPartialVersion.Parse(Match.Groups[1].Value);
        }
        catch (ArgumentException) {
            return null;
        }

        // If partial version match is actually a full version,
        // then this isn't a star range, so return null.
        if (version.IsFull()) {
            return null;
        }

        SemanticVersion minVersion = null;
        SemanticVersion maxVersion = null;
        if (!version.Major.HasValue) {
            minVersion = version.ToZeroVersion();
            // no max version
        }
        else if (!version.Minor.HasValue) {
            minVersion = version.ToZeroVersion();
            maxVersion = new Version(version.Major.Value + 1, 0, 0);
        }
        else {
            minVersion = version.ToZeroVersion();
            maxVersion = new Version(version.Major.Value, version.Minor.Value + 1, 0);
        }

        return (Match.Length,
            new SemanticVersionComparator(SemanticVersionOperator.GreaterThanOrEqualIncludingPrereleases, minVersion),
            new SemanticVersionComparator(SemanticVersionOperator.LessThanExcludingPrereleases, maxVersion));
    }

    private static SemanticVersionComparator[] MinMaxComparators(SemanticVersion minVersion, SemanticVersion maxVersion,
            SemanticVersionOperator minOperator = SemanticVersionOperator.GreaterThanOrEqual,
            SemanticVersionOperator maxOperator = SemanticVersionOperator.LessThanExcludingPrereleases) {

        SemanticVersionComparator minComparator = new(minOperator, minVersion);
        if (maxVersion == null) {
            return [minComparator];
        }
        else {
            var maxComparator = new SemanticVersionComparator(maxOperator, maxVersion);
            return [minComparator, maxComparator];
        }
    }
}