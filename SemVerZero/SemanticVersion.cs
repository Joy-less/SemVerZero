using System.ComponentModel;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace SemVerZero;

/// <summary>
/// A semantic version number in full compliance with the <see href="https://semver.org">Semantic Versioning 2.0.0</see> specification.
/// </summary>
public readonly struct SemanticVersion : IComparable, IComparable<SemanticVersion>, IEquatable<SemanticVersion>, IFormattable
#if !NETSTANDARD
    , IComparisonOperators<SemanticVersion, SemanticVersion, bool>, ISpanFormattable, IUtf8SpanFormattable
#endif
    {
    /// <summary>
    /// The <c>MAJOR</c> version, to be incremented when you make incompatible API changes. This is the first integer.
    /// </summary>
    public long Major { get; } = 0;
    /// <summary>
    /// The <c>MINOR</c> version, to be incremented when you add functionality in a backward compatible manner. This is the second integer.
    /// </summary>
    public long Minor { get; } = 0;
    /// <summary>
    /// The <c>PATCH</c> version, to be incremented when you make backward compatible bug fixes. This is the third integer.
    /// </summary>
    public long Patch { get; } = 0;
    /// <summary>
    /// The pre-release metadata, for example <c>alpha.1</c>. This is the text after the hyphen (<c>-</c>).
    /// </summary>
    public string? Prerelease { get; } = null;
    /// <summary>
    /// The build metadata, for example <c>001</c>. This is the text after the plus (<c>+</c>).
    /// </summary>
    public string? Build { get; } = null;

    /// <summary>
    /// Constructs a version for <c>0.0.0</c>.
    /// </summary>
    public SemanticVersion()
        : this(0) {
    }
    /// <summary>
    /// Constructs a version for <c><paramref name="Major"/>.0.0-<paramref name="Prerelease"/>+<paramref name="Build"/></c>.
    /// </summary>
    public SemanticVersion(long Major, string? Prerelease = null, string? Build = null)
        : this(Major, 0, Prerelease, Build) {
    }
    /// <summary>
    /// Constructs a version for <c><paramref name="Major"/>.<paramref name="Minor"/>.0-<paramref name="Prerelease"/>+<paramref name="Build"/></c>.
    /// </summary>
    public SemanticVersion(long Major, long Minor, string? Prerelease = null, string? Build = null)
        : this(Major, Minor, 0, Prerelease, Build) {
    }
    /// <summary>
    /// Constructs a version for <c><paramref name="Major"/>.<paramref name="Minor"/>.<paramref name="Patch"/>-<paramref name="Prerelease"/>+<paramref name="Build"/></c>.
    /// </summary>
    public SemanticVersion(long Major, long Minor, long Patch, string? Prerelease = null, string? Build = null) {
        if (Major < 0) throw new ArgumentOutOfRangeException(nameof(Major));
        if (Minor < 0) throw new ArgumentOutOfRangeException(nameof(Minor));
        if (Patch < 0) throw new ArgumentOutOfRangeException(nameof(Patch));
        if (Prerelease is not null) ValidateTag(Prerelease);
        if (Build is not null) ValidateTag(Build);

        this.Major = Major;
        this.Minor = Minor;
        this.Patch = Patch;
        this.Prerelease = Prerelease;
        this.Build = Build;
    }

    /// <summary>
    /// Converts the version to a string in the <see cref="SemanticVersionFormat.MajorMinorPatch"/> format.
    /// </summary>
    public override string ToString() {
        return ToString(SemanticVersionFormat.MajorMinorPatch);
    }
    /// <summary>
    /// Converts the version to a string in the given format.
    /// </summary>
    public string ToString(SemanticVersionFormat Format) {
#if NETSTANDARD
        if (!Enum.IsDefined(typeof(SemanticVersionFormat), Format)) throw new InvalidEnumArgumentException(nameof(Format));
#else
        if (!Enum.IsDefined(Format)) throw new InvalidEnumArgumentException(nameof(Format));
#endif

        return ToStringCore(
            IncludeMinor: Format >= SemanticVersionFormat.MajorMinor || Minor != 0 || Patch != 0,
            IncludePatch: Format >= SemanticVersionFormat.MajorMinorPatch || Patch != 0,
            IncludePrelease: Prerelease is not null,
            IncludeBuild: Build is not null
        );
    }
    /// <inheritdoc cref="ToString()"/>
    string IFormattable.ToString(string? Format, IFormatProvider? Provider) {
        // Format and provider are ignored
        return ToString();
    }
#if !NETSTANDARD
    /// <summary>
    /// Converts the version to a string in the <see cref="SemanticVersionFormat.MajorMinorPatch"/> format.
    /// </summary>
    public bool TryFormat(Span<char> Destination, out int CharsWritten) {
        return TryFormatCore(Destination, out CharsWritten,
            IncludeMinor: true,
            IncludePatch: true,
            IncludePrelease: Prerelease is not null,
            IncludeBuild: Build is not null
        );
    }
    /// <summary>
    /// Converts the version to a string in the given format.
    /// </summary>
    public bool TryFormat(Span<char> Destination, out int CharsWritten, SemanticVersionFormat Format) {
        if (!Enum.IsDefined(Format)) throw new InvalidEnumArgumentException(nameof(Format));

        return TryFormatCore(Destination, out CharsWritten,
            IncludeMinor: Format >= SemanticVersionFormat.MajorMinor || Minor != 0 || Patch != 0,
            IncludePatch: Format >= SemanticVersionFormat.MajorMinorPatch || Patch != 0,
            IncludePrelease: Prerelease is not null,
            IncludeBuild: Build is not null
        );
    }
    /// <inheritdoc cref="TryFormat(Span{char}, out int)"/>
    bool ISpanFormattable.TryFormat(Span<char> Destination, out int CharsWritten, scoped ReadOnlySpan<char> Format, IFormatProvider? Provider) {
        // Format and provider are ignored
        return TryFormat(Destination, out CharsWritten);
    }
    /// <summary>
    /// Converts the version to a UTF-8 string in the <see cref="SemanticVersionFormat.MajorMinorPatch"/> format.
    /// </summary>
    public bool TryFormat(Span<byte> Utf8Destination, out int BytesWritten) {
        return TryFormatCore(Utf8Destination, out BytesWritten,
            IncludeMinor: true,
            IncludePatch: true,
            IncludePrelease: Prerelease is not null,
            IncludeBuild: Build is not null
        );
    }
    /// <summary>
    /// Converts the version to a UTF-8 string in the given format.
    /// </summary>
    public bool TryFormat(Span<byte> Utf8Destination, out int BytesWritten, SemanticVersionFormat Format) {
        if (!Enum.IsDefined(Format)) throw new InvalidEnumArgumentException(nameof(Format));

        return TryFormatCore(Utf8Destination, out BytesWritten,
            IncludeMinor: Format >= SemanticVersionFormat.MajorMinor || Minor != 0 || Patch != 0,
            IncludePatch: Format >= SemanticVersionFormat.MajorMinorPatch || Patch != 0,
            IncludePrelease: Prerelease is not null,
            IncludeBuild: Build is not null
        );
    }
    /// <inheritdoc cref="TryFormat(Span{byte}, out int)"/>
    bool IUtf8SpanFormattable.TryFormat(Span<byte> Utf8Destination, out int BytesWritten, scoped ReadOnlySpan<char> Format, IFormatProvider? Provider) {
        // Format and provider are ignored
        return TryFormat(Utf8Destination, out BytesWritten);
    }
#endif
    /// <summary>
    /// Creates a hash-code for the version, including <see cref="Major"/>, <see cref="Minor"/>, <see cref="Patch"/> and <see cref="Prerelease"/>.
    /// </summary>
    public override int GetHashCode() {
        unchecked {
            int Hash = 17;

            Hash = (Hash * 23) + Major.GetHashCode();
            Hash = (Hash * 23) + Minor.GetHashCode();
            Hash = (Hash * 23) + Patch.GetHashCode();

            if (Prerelease is not null) {
                Hash = (Hash * 23) + Prerelease.GetHashCode();
            }

            // Ignore build, since two semantic versions with same build are equal

            return Hash;
        }
    }
    /// <summary>
    /// Checks whether <paramref name="Other"/> is a <see cref="SemanticVersion"/> equal to this version.
    /// </summary>
    public override bool Equals(object? Other) {
        return Other is SemanticVersion OtherSemanticVersion && Equals(OtherSemanticVersion);
    }
    /// <summary>
    /// Checks whether <paramref name="Other"/> is equal to this version.
    /// </summary>
    public bool Equals(SemanticVersion Other) {
        return CompareTo(Other) == 0;
    }
    /// <summary>
    /// Compares this version to the other version, returning:
    /// <list type="bullet">
    ///   <item><c>1</c> if this version is greater than the other version</item>
    ///   <item><c>0</c> if this version is equal to the other version</item>
    ///   <item><c>-1</c> if this version is less than the other version</item>
    ///   <item><c>1</c> if the other version is <see langword="null"/></item>
    /// </list>
    /// </summary>
    public int CompareTo(object? Other) {
        return Other switch {
            null => 1,
            SemanticVersion SemanticVersion => CompareTo(SemanticVersion),
            _ => throw new ArgumentException($"{nameof(Other)} is not {nameof(SemanticVersion)}", nameof(Other)),
        };
    }
    /// <summary>
    /// Compares this version to the other version, returning:
    /// <list type="bullet">
    ///   <item><c>1</c> if this version is greater than the other version</item>
    ///   <item><c>0</c> if this version is equal to the other version</item>
    ///   <item><c>-1</c> if this version is less than the other version</item>
    /// </list>
    /// </summary>
    public int CompareTo(SemanticVersion Other) {
        int MajorComparison = Major.CompareTo(Other.Major);
        if (MajorComparison != 0) {
            return MajorComparison;
        }

        int MinorComparison = Minor.CompareTo(Other.Minor);
        if (MinorComparison != 0) {
            return MinorComparison;
        }

        int PatchComparison = Patch.CompareTo(Other.Patch);
        if (PatchComparison != 0) {
            return PatchComparison;
        }

        if (Prerelease is null) {
            if (Other.Prerelease is null) {
                return 0;
            }
            else {
                return 1;
            }
        }
        if (Other.Prerelease is null) {
            return -1;
        }

        return CompareTags(Prerelease, Other.Prerelease);
    }

    /// <summary>
    /// Converts the input string to a <see cref="SemanticVersion"/>, throwing on failure.
    /// </summary>
    /// <exception cref="FormatException"></exception>
    public static SemanticVersion Parse(scoped ReadOnlySpan<char> Input) {
        long Major = 0;
        long Minor = 0;
        long Patch = 0;
        string? Prerelease = null;
        string? Build = null;

        int Index = 0;

        static long ParseInteger(scoped ReadOnlySpan<char> Input, ref int Index) {
            int StartIndex = Index;

            for (; Index < Input.Length; Index++) {
                char Char = Input[Index];

                // Dot / Hyphen / Plus
                if (Char is '.' or '-' or '+') {
                    break;
                }
                // Digit
                else if (Char is >= '0' and <= '9') {
                }
                // Invalid
                else {
                    throw new FormatException("Invalid pre-release character (expected [0-9A-Za-z-])");
                }
            }

#if NETSTANDARD2_0
            return long.Parse(Input[StartIndex..Index].ToString(), NumberStyles.None);
#else
            return long.Parse(Input[StartIndex..Index], NumberStyles.None);
#endif
        }
        static ReadOnlySpan<char> ParsePrerelease(ReadOnlySpan<char> Input, ref int Index) {
            int StartIndex = Index;
            bool Dot = false;

            for (; Index < Input.Length; Index++) {
                char Char = Input[Index];

                // Plus
                if (Char is '+') {
                    Dot = false;
                    break;
                }
                // Dot
                else if (Char is '.') {
                    if (Dot) {
                        throw new FormatException("Empty pre-release identifier");
                    }
                    Dot = true;
                }
                // Alphanumeric character / Hyphen
                else if ((Char is >= '0' and <= '9') || (Char is >= 'A' and <= 'Z') || (Char is >= 'a' and <= 'z') || Char is '-') {
                    Dot = false;
                }
                // Invalid
                else {
                    throw new FormatException("Invalid pre-release character (expected [0-9A-Za-z-])");
                }
            }

            if (Dot) {
                throw new FormatException("Empty pre-release identifier");
            }

            return Input[StartIndex..Index];
        }
        static ReadOnlySpan<char> ParseBuild(ReadOnlySpan<char> Input, ref int Index) {
            int StartIndex = Index;
            bool Dot = false;

            for (; Index < Input.Length; Index++) {
                char Char = Input[Index];

                // Dot
                if (Char is '.') {
                    if (Dot) {
                        throw new FormatException("Empty build identifier");
                    }
                    Dot = true;
                }
                // Alphanumeric character / Hyphen
                else if ((Char is >= '0' and <= '9') || (Char is >= 'A' and <= 'Z') || (Char is >= 'a' and <= 'z') || Char is '-') {
                    Dot = false;
                }
                // Invalid
                else {
                    throw new FormatException("Invalid build character (expected [0-9A-Za-z-])");
                }
            }

            if (Dot) {
                throw new FormatException("Empty build identifier");
            }

            return Input[StartIndex..Index];
        }

        // Major
        Major = ParseInteger(Input, ref Index);

        // Minor
        if (Index < Input.Length && Input[Index] == '.') {
            Index++;
            Minor = ParseInteger(Input, ref Index);

            // Patch
            if (Index < Input.Length && Input[Index] == '.') {
                Index++;
                Patch = ParseInteger(Input, ref Index);
            }
        }

        // Prerelease
        if (Index < Input.Length && Input[Index] == '-') {
            Index++;
            Prerelease = ParsePrerelease(Input, ref Index).ToString();
        }

        // Build
        if (Index < Input.Length && Input[Index] == '+') {
            Index++;
            Build = ParseBuild(Input, ref Index).ToString();
        }

        return new SemanticVersion(Major, Minor, Patch, Prerelease, Build);
    }
    /// <summary>
    /// Converts the input string to a <see cref="SemanticVersion"/>, returning <see langword="false"/> on failure.
    /// </summary>
    public static bool TryParse(scoped ReadOnlySpan<char> Input, out SemanticVersion Result) {
        long Major = 0;
        long Minor = 0;
        long Patch = 0;
        string? Prerelease = null;
        string? Build = null;

        int Index = 0;

        static bool TryParseInteger(scoped ReadOnlySpan<char> Input, ref int Index, out long Result) {
            int StartIndex = Index;

            for (; Index < Input.Length; Index++) {
                char Char = Input[Index];

                // Dot / Hyphen / Plus
                if (Char is '.' or '-' or '+') {
                    break;
                }
                // Digit
                else if (Char is >= '0' and <= '9') {
                }
                // Invalid
                else {
                    Result = default;
                    return false;
                }
            }

#if NETSTANDARD2_0
            return long.TryParse(Input[StartIndex..Index].ToString(), NumberStyles.None, null, out Result);
#else
            return long.TryParse(Input[StartIndex..Index], NumberStyles.None, null, out Result);
#endif
        }
        static bool TryParsePrerelease(ReadOnlySpan<char> Input, ref int Index, out ReadOnlySpan<char> Result) {
            int StartIndex = Index;
            bool Dot = false;

            for (; Index < Input.Length; Index++) {
                char Char = Input[Index];

                // Plus
                if (Char is '+') {
                    break;
                }
                // Dot
                else if (Char is '.') {
                    if (Dot) {
                        throw new FormatException("Empty pre-release identifier");
                    }
                    Dot = true;
                }
                // Alphanumeric character / Hyphen
                else if ((Char is >= '0' and <= '9') || (Char is >= 'A' and <= 'Z') || (Char is >= 'a' and <= 'z') || Char is '-') {
                    Dot = false;
                }
                // Invalid
                else {
                    Result = default;
                    return false;
                }
            }

            if (Dot) {
                throw new FormatException("Empty pre-release identifier");
            }

            Result = Input[StartIndex..Index];
            return true;
        }
        static bool TryParseBuild(ReadOnlySpan<char> Input, ref int Index, out ReadOnlySpan<char> Result) {
            int StartIndex = Index;
            bool Dot = false;

            for (; Index < Input.Length; Index++) {
                char Char = Input[Index];

                // Dot
                if (Char is '.') {
                    if (Dot) {
                        throw new FormatException("Empty build identifier");
                    }
                    Dot = true;
                }
                // Alphanumeric character / Hyphen
                else if ((Char is >= '0' and <= '9') || (Char is >= 'A' and <= 'Z') || (Char is >= 'a' and <= 'z') || Char is '-') {
                    Dot = false;
                }
                // Invalid
                else {
                    Result = default;
                    return false;
                }
            }

            if (Dot) {
                throw new FormatException("Empty build identifier");
            }

            Result = Input[StartIndex..Index];
            return true;
        }

        // Major
        if (!TryParseInteger(Input, ref Index, out Major)) {
            Result = default;
            return false;
        }

        // Minor
        if (Index < Input.Length && Input[Index] == '.') {
            Index++;
            if (!TryParseInteger(Input, ref Index, out Minor)) {
                Result = default;
                return false;
            }

            // Patch
            if (Index < Input.Length && Input[Index] == '.') {
                Index++;
                if (!TryParseInteger(Input, ref Index, out Patch)) {
                    Result = default;
                    return false;
                }
            }
        }

        // Prerelease
        if (Index < Input.Length && Input[Index] == '-') {
            Index++;
            if (!TryParsePrerelease(Input, ref Index, out ReadOnlySpan<char> PrereleaseSpan)) {
                Result = default;
                return false;
            }
            Prerelease = PrereleaseSpan.ToString();
        }

        // Build
        if (Index < Input.Length && Input[Index] == '+') {
            Index++;
            if (!TryParseBuild(Input, ref Index, out ReadOnlySpan<char> BuildSpan)) {
                Result = default;
                return false;
            }
            Build = BuildSpan.ToString();
        }

        Result = new SemanticVersion(Major, Minor, Patch, Prerelease, Build);
        return true;
    }

    private static void ValidateTag(scoped ReadOnlySpan<char> Tag) {
        int IdentifierStartIndex = 0;

        for (int Index = 0; Index < Tag.Length; Index++) {
            char Char = Tag[Index];

            // Dot
            if (Char is '.') {
                if (IdentifierStartIndex == Index) {
                    throw new FormatException("Empty tag identifier");
                }

                IdentifierStartIndex = Index + 1;
            }
            // Alphanumeric character / Hyphen
            else if ((Char is >= '0' and <= '9') || (Char is >= 'A' and <= 'Z') || (Char is >= 'a' and <= 'z') || Char is '-') {
            }
            // Invalid
            else {
                throw new FormatException("Invalid tag character (expected [0-9A-Za-z-])");
            }
        }

        if (IdentifierStartIndex == Tag.Length) {
            throw new FormatException("Empty tag identifier");
        }
    }
    private static int CompareTags(scoped ReadOnlySpan<char> TagA, scoped ReadOnlySpan<char> TagB) {
        int TagAIndex = 0;
        int TagBIndex = 0;

        static ReadOnlySpan<char> GetNextIdentifier(ReadOnlySpan<char> Tag, ref int Index) {
            int StartIndex = Index;

            for (; Index < Tag.Length; Index++) {
                char Char = Tag[Index];

                // Dot
                if (Char is '.') {
                    break;
                }
            }

            ReadOnlySpan<char> Identifier = Tag[StartIndex..Index];

            // Move past dot
            if (Index + 1 < Tag.Length) {
                Index++;
            }

            return Identifier;
        }

        while (true) {
            ReadOnlySpan<char> IdentifierA = GetNextIdentifier(TagA, ref TagAIndex);
            ReadOnlySpan<char> IdentifierB = GetNextIdentifier(TagB, ref TagBIndex);

            if (IdentifierA.IsEmpty) {
                if (IdentifierB.IsEmpty) {
                    return 0;
                }
                else {
                    return -1;
                }
            }
            if (IdentifierB.IsEmpty) {
                return 1;
            }

#if NETSTANDARD2_0
            if (long.TryParse(IdentifierA.ToString(), NumberStyles.None, null, out long IntegerA) && long.TryParse(IdentifierB.ToString(), NumberStyles.None, null, out long IntegerB)) {
#else
            if (long.TryParse(IdentifierA, NumberStyles.None, null, out long IntegerA) && long.TryParse(IdentifierB, NumberStyles.None, null, out long IntegerB)) {
#endif
                int IntegerABComparison = IntegerA.CompareTo(IntegerB);
                if (IntegerABComparison != 0) {
                    return IntegerABComparison;
                }
            }
            else {
                int IdentifierABComparison = IdentifierA.CompareTo(IdentifierB, StringComparison.Ordinal);
                if (IdentifierABComparison != 0) {
                    return IdentifierABComparison;
                }
            }
        }
    }

    private string ToStringCore(bool IncludeMinor, bool IncludePatch, bool IncludePrelease, bool IncludeBuild) {
        return Major
            + (IncludeMinor ? ("." + Minor) : "")
            + (IncludePatch ? ("." + Patch) : "")
            + (IncludePrelease ? ("-" + Prerelease) : "")
            + (IncludeBuild ? ("+" + Build) : "");
    }
#if !NETSTANDARD
    private bool TryFormatCore(Span<char> Destination, out int CharsWritten, bool IncludeMinor, bool IncludePatch, bool IncludePrelease, bool IncludeBuild) {
        CharsWritten = 0;

        // Major
        if (!Major.TryFormat(Destination, out int MajorCharsWritten)) {
            CharsWritten += MajorCharsWritten;
            return false;
        }
        CharsWritten += MajorCharsWritten;

        // Minor
        if (IncludeMinor) {
            // Dot
            if (CharsWritten == Destination.Length) {
                return false;
            }
            Destination[CharsWritten] = '.';
            CharsWritten++;

            // Minor
            if (!Minor.TryFormat(Destination[CharsWritten..], out int MinorCharsWritten)) {
                CharsWritten += MinorCharsWritten;
                return false;
            }
            CharsWritten += MinorCharsWritten;
        }

        // Patch
        if (IncludePatch) {
            // Dot
            if (CharsWritten == Destination.Length) {
                return false;
            }
            Destination[CharsWritten] = '.';
            CharsWritten++;

            // Patch
            if (!Patch.TryFormat(Destination[CharsWritten..], out int PatchCharsWritten)) {
                CharsWritten += PatchCharsWritten;
                return false;
            }
            CharsWritten += PatchCharsWritten;
        }

        // Prerelease
        if (IncludePrelease) {
            // Hyphen
            if (CharsWritten == Destination.Length) {
                return false;
            }
            Destination[CharsWritten] = '-';
            CharsWritten++;

            // Prerelease
            if (Prerelease is not null) {
                if (!Prerelease.TryCopyTo(Destination[CharsWritten..])) {
                    return false;
                }
                CharsWritten += Prerelease.Length;
            }
        }

        // Build
        if (IncludeBuild) {
            // Plus
            if (CharsWritten == Destination.Length) {
                return false;
            }
            Destination[CharsWritten] = '+';
            CharsWritten++;

            // Build
            if (Build is not null) {
                if (!Build.TryCopyTo(Destination[CharsWritten..])) {
                    return false;
                }
                CharsWritten += Build.Length;
            }
        }

        return true;
    }
    private bool TryFormatCore(Span<byte> Utf8Destination, out int BytesWritten, bool IncludeMinor, bool IncludePatch, bool IncludePrelease, bool IncludeBuild) {
        BytesWritten = 0;

        // Major
        if (!Major.TryFormat(Utf8Destination, out int MajorBytesWritten)) {
            BytesWritten += MajorBytesWritten;
            return false;
        }
        BytesWritten += MajorBytesWritten;

        // Minor
        if (IncludeMinor) {
            // Dot
            if (BytesWritten == Utf8Destination.Length) {
                return false;
            }
            Utf8Destination[BytesWritten] = (byte)'.';
            BytesWritten++;

            // Minor
            if (!Minor.TryFormat(Utf8Destination[BytesWritten..], out int MinorBytesWritten)) {
                BytesWritten += MinorBytesWritten;
                return false;
            }
            BytesWritten += MinorBytesWritten;
        }

        // Patch
        if (IncludePatch) {
            // Dot
            if (BytesWritten == Utf8Destination.Length) {
                return false;
            }
            Utf8Destination[BytesWritten] = (byte)'.';
            BytesWritten++;

            // Patch
            if (!Patch.TryFormat(Utf8Destination[BytesWritten..], out int PatchBytesWritten)) {
                BytesWritten += PatchBytesWritten;
                return false;
            }
            BytesWritten += PatchBytesWritten;
        }

        // Prerelease
        if (IncludePrelease) {
            // Hyphen
            if (BytesWritten == Utf8Destination.Length) {
                return false;
            }
            Utf8Destination[BytesWritten] = (byte)'-';
            BytesWritten++;

            // Prerelease
            if (Prerelease is not null) {
                if (!Encoding.UTF8.TryGetBytes(Prerelease, Utf8Destination[BytesWritten..], out int PrereleaseBytesWritten)) {
                    BytesWritten += PrereleaseBytesWritten;
                    return false;
                }
                BytesWritten += PrereleaseBytesWritten;
            }
        }

        // Build
        if (IncludeBuild) {
            // Plus
            if (BytesWritten == Utf8Destination.Length) {
                return false;
            }
            Utf8Destination[BytesWritten] = (byte)'+';
            BytesWritten++;

            // Build
            if (Build is not null) {
                if (!Encoding.UTF8.TryGetBytes(Build, Utf8Destination[BytesWritten..], out int BuildBytesWritten)) {
                    BytesWritten += BuildBytesWritten;
                    return false;
                }
                BytesWritten += BuildBytesWritten;
            }
        }

        return true;
    }
#endif

    /// <summary>
    /// Returns <see langword="true"/> if <paramref name="Left"/> is greater than <paramref name="Right"/>.
    /// </summary>
    public static bool operator >(SemanticVersion Left, SemanticVersion Right) {
        return Left.CompareTo(Right) > 0;
    }
    /// <summary>
    /// Returns <see langword="true"/> if <paramref name="Left"/> is greater than or equal to <paramref name="Right"/>.
    /// </summary>
    public static bool operator >=(SemanticVersion Left, SemanticVersion Right) {
        return Left.CompareTo(Right) >= 0;
    }
    /// <summary>
    /// Returns <see langword="true"/> if <paramref name="Left"/> is less than <paramref name="Right"/>.
    /// </summary>
    public static bool operator <(SemanticVersion Left, SemanticVersion Right) {
        return Left.CompareTo(Right) < 0;
    }
    /// <summary>
    /// Returns <see langword="true"/> if <paramref name="Left"/> is less than or equal to <paramref name="Right"/>.
    /// </summary>
    public static bool operator <=(SemanticVersion Left, SemanticVersion Right) {
        return Left.CompareTo(Right) <= 0;
    }
    /// <summary>
    /// Returns <see langword="true"/> if <paramref name="Left"/> is equal to <paramref name="Right"/>.
    /// </summary>
    public static bool operator ==(SemanticVersion Left, SemanticVersion Right) {
        return Left.Equals(Right);
    }
    /// <summary>
    /// Returns <see langword="true"/> if <paramref name="Left"/> is not equal to <paramref name="Right"/>.
    /// </summary>
    public static bool operator !=(SemanticVersion Left, SemanticVersion Right) {
        return !Left.Equals(Right);
    }
}