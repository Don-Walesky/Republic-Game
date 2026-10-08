namespace Republic.Core.World.Rules;

using System;
using System.Text.RegularExpressions;

/// <summary>
/// Domain rule implementation enforcing that every player country name must contain the word 'Republic'.
/// Matches the whole word 'Republic' case-insensitively. Rejects empty, whitespace, and non-conforming names.
/// Does not silently rewrite names.
/// </summary>
public sealed class CountryNameRule : ICountryNameRule
{
    /// <summary>
    /// The mandatory keyword required as a standalone word in every country name.
    /// </summary>
    public const string RequiredKeyword = "Republic";

    private static readonly Regex RepublicWordRegex = new(
        @"\bRepublic\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    /// <summary>
    /// Gets the singleton default instance of the rule.
    /// </summary>
    public static CountryNameRule Default { get; } = new();

    /// <inheritdoc />
    public bool IsValid(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        return RepublicWordRegex.IsMatch(name.Trim());
    }

    /// <inheritdoc />
    public string Validate(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Country name cannot be empty or whitespace.", nameof(name));
        }

        var trimmed = name.Trim();
        if (!RepublicWordRegex.IsMatch(trimmed))
        {
            throw new ArgumentException(
                $"Country name '{name}' is invalid. Every player country name must contain the whole word '{RequiredKeyword}'.",
                nameof(name));
        }

        return trimmed;
    }

    /// <summary>
    /// Static convenience method to test validity of a country name.
    /// </summary>
    public static bool Check(string? name) => Default.IsValid(name);

    /// <summary>
    /// Static convenience method to validate and trim a country name.
    /// </summary>
    public static string CleanAndValidate(string? name) => Default.Validate(name);
}
