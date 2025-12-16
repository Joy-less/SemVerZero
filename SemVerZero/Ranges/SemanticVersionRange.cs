namespace SemVerZero.Ranges;

public readonly struct SemanticVersionRange {
    public IEnumerable<SemanticVersionComparatorSet>? ComparatorSets { get; }

    public SemanticVersionRange() {
    }
    public SemanticVersionRange(params IEnumerable<SemanticVersionComparatorSet>? ComparatorSets) {
        this.ComparatorSets = ComparatorSets;
    }

    public bool IsMatch(SemanticVersion Version) {
        if (ComparatorSets is not null) {
            foreach (SemanticVersionComparatorSet ComparatorSet in ComparatorSets) {
                if (!ComparatorSet.IsMatch(Version)) {
                    return false;
                }
            }
        }
        return true;
    }

    public static SemanticVersionRange Parse(scoped ReadOnlySpan<char> Input) {
        // Split input by "||"
        Span<Range> SplitRanges = stackalloc Range[Input.Length];
        int SplitRangesCount = Input.Split(SplitRanges, "||", StringSplitOptions.None);
        ReadOnlySpan<Range> SplitRangesReadOnly = SplitRanges[..SplitRangesCount];

        // Create comparator sets for each split
        SemanticVersionComparatorSet[] ComparatorSets = new SemanticVersionComparatorSet[SplitRangesReadOnly.Length];
        for (int Index = 0; Index < ComparatorSets.Length; Index++) {
            Range SplitRange = SplitRangesReadOnly[Index];
            ReadOnlySpan<char> SplitPart = Input[SplitRange];

            ComparatorSets[Index] = SemanticVersionComparatorSet.Parse(SplitPart);
        }

        // Create range from comparator sets
        return new SemanticVersionRange(ComparatorSets);
    }
}