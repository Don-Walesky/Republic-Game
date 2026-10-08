namespace Republic.Core.World.Rules;

/// <summary>
/// Domain rule defining validity constraints for sovereign country names.
/// Enforces that every sovereign country name must contain the whole word 'Republic'.
/// </summary>
public interface ICountryNameRule
{
    /// <summary>
    /// Determines whether the specified country name contains the whole word 'Republic' (case-insensitive) after trimming.
    /// </summary>
    /// <param name="name">The proposed country name.</param>
    /// <returns><c>true</c> if valid; otherwise, <c>false</c>.</returns>
    bool IsValid(string? name);

    /// <summary>
    /// Validates and trims the country name. Throws an <see cref="ArgumentException"/> if invalid.
    /// Does not silently rewrite missing keywords.
    /// </summary>
    /// <param name="name">The proposed country name.</param>
    /// <returns>The trimmed, validated country name.</returns>
    /// <exception cref="ArgumentException">Thrown when the name is null, whitespace, or does not contain the whole word 'Republic'.</exception>
    string Validate(string? name);
}
