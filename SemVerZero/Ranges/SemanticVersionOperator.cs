namespace SemVerZero.Ranges;

public enum SemanticVersionOperator {
    Equal = 0,
    LessThan = 1,
    LessThanOrEqual = 2,
    GreaterThan = 3,
    GreaterThanOrEqual = 4,
    GreaterThanOrEqualIncludingPrereleases = 5,
    LessThanExcludingPrereleases = 6,
}