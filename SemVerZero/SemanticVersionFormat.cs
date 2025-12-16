namespace SemVerZero;

/// <summary>
/// The format to display a semantic version.
/// </summary>
public enum SemanticVersionFormat {
    /// <summary>
    /// The format <c>MAJOR</c>. This hides <c>MINOR</c> and <c>PATCH</c> if they are 0.
    /// </summary>
    Major = 0,
    /// <summary>
    /// The format <c>MAJOR.MINOR</c>. This hides <c>PATCH</c> if it is 0.
    /// </summary>
    MajorMinor = 1,
    /// <summary>
    /// The format <c>MAJOR.MINOR.PATCH</c>.
    /// </summary>
    MajorMinorPatch = 2,
}