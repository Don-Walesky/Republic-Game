namespace Republic.Core.Modding.Models;

using System;
using System.Collections.Generic;

/// <summary>
/// Domain model describing a custom mod package or workshop scenario extension.
/// </summary>
public sealed class ModManifest
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0";
    public string Description { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public List<string> CustomScenarioFiles { get; set; } = new();
}
