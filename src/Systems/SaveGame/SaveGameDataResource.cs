namespace Codartesien.Game.Systems.SaveGame;

using Godot;

/// <summary>
/// Resource version of the save game data that will be propagated to the game systems through signals.
/// Godot signals do not support raw C# types, so we need to use resources to pass data around.
/// </summary>
public partial class SaveGameDataResource : Resource
{
    public string SavedAt { get; set; } = string.Empty;

    public bool CharacterCreated { get; set; }
    
    public string CharacterName { get; set; }
    
    public Vector2 CharacterPosition { get; set; }
    
    public string CharacterDirection { get; set; }
}
