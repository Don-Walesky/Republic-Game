namespace Republic.Core.Workspace.Models;

using System.Collections.Generic;

/// <summary>
/// State of the executive room and environment.
/// </summary>
public sealed class OfficeRoomState
{
    public string ActiveRoomName { get; set; } = "Executive Office";
    public string LightingMode { get; set; } = "Day";
    public string AmbientAudioZone { get; set; } = "DeskAmbience";

    /// <summary>
    /// Collection of unique furniture, decor, and customization items owned by the executive office.
    /// </summary>
    public List<string> OwnedFurniture { get; set; } = new();
}
