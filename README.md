# SemVerZero

[![NuGet](https://img.shields.io/nuget/v/SemVerZero.svg)](https://www.nuget.org/packages/SemVerZero)

Zero-allocation semantic versioning for C#.

This library provides a performant `SemanticVersion` type in full compliance with the [Semantic Versioning 2.0.0](https://semver.org) specification.

## Example

```cs
SemanticVersion Version = SemanticVersion.Parse("1.0.0-rc.1");

long Major = Version.Major;
long Minor = Version.Minor;
long Patch = Version.Patch;
string? Prerelease = Version.Prerelease;
string? Build = Version.Build;

string String = Version.ToString();
string MinimalString = Version.ToString(SemanticVersionFormat.Major);
```

## Benchmarks

Comparison between [SemVerZero](https://github.com/Joy-less/SemVerZero), [adamreeve/semver.net](https://github.com/adamreeve/semver.net) and [WalkerCodeRanger/semver](https://github.com/WalkerCodeRanger/semver):

| Method                 | Mean      | Error    | StdDev   | Gen0   | Allocated |
|----------------------- |----------:|---------:|---------:|-------:|----------:|
| Parse_SemVerZero       |  69.29 ns | 0.198 ns | 0.185 ns | 0.0063 |      40 B |
| Parse_adamreeve        | 987.76 ns | 2.038 ns | 1.701 ns | 0.2518 |    1584 B |
| Parse_WalkerCodeRanger | 554.70 ns | 3.628 ns | 3.394 ns | 0.0916 |     576 B |

| Method                    | Mean      | Error    | StdDev   | Gen0   | Allocated |
|-------------------------- |----------:|---------:|---------:|-------:|----------:|
| ToString_SemVerZero       |  95.20 ns | 0.170 ns | 0.151 ns | 0.0356 |     224 B |
| ToString_adamreeve        | 390.26 ns | 1.067 ns | 0.946 ns | 0.1082 |     680 B |
| ToString_WalkerCodeRanger | 416.89 ns | 3.960 ns | 3.705 ns | 0.1183 |     744 B |

## Notes

- The Major, Minor and Patch versions are typed as `long` (64-bit signed integer). So, the maximum value of each version is `9,223,372,036,854,775,807`.
- Versions in the [ZeroVer](https://0ver.org) format are accepted.