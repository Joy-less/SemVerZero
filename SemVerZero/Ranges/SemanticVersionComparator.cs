namespace SemVerZero.Ranges;

public readonly struct SemanticVersionComparator {
    public SemanticVersionOperator Operator { get; }
    public SemanticVersion Version { get; }

    public SemanticVersionComparator() {
    }
    public SemanticVersionComparator(SemanticVersion ExactVersion)
        : this(SemanticVersionOperator.Equal, ExactVersion) {
    }
    public SemanticVersionComparator(SemanticVersionOperator Operator, SemanticVersion Version) {
        this.Operator = Operator;
        this.Version = Version;
    }

    public bool IsMatch(SemanticVersion Version) {

    }

    public static SemanticVersionComparator Parse(scoped ReadOnlySpan<char> Input) {
        
    }
}