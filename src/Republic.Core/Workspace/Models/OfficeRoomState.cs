namespace Republic.Core.Workspace.Models;

/// <summary>
/// State of the executive room and environment.
/// </summary>
public sealed class OfficeRoomState
{
    public string ActiveRoomName { get; set; } = "Executive Office";
    public string LightingMode { get; set; } = "Day";
    public string AmbientAudioZone { get; set; } = "DeskAmbience";
}
