namespace Codartesien.Game.Systems.SaveGame;

using Godot;

/// <summary>
/// Current save game data object.
/// 
/// If the save game format changes:
/// - copy/paste this content into a new SaveGameDataVersionX object.
/// - remove the ToResource method from the SaveGameDataVersionX object.
/// - update the SaveGameData object to match the new save game format.
/// </summary>
public class SaveGameData
{
    public int SaveVersion { get; init; }

    public string SavedAt { get; init; } = string.Empty;
    
    public bool CharacterCreated { get; init; }

    public string CharacterName { get; init; } = "Player";

    public float CharacterPositionX { get; init; }
    
    public float CharacterPositionY { get; init; }

    public string CharacterDirection { get; init; } = "down";

    public SaveGameDataResource ToResource()
    {
        return new SaveGameDataResource {
            SavedAt = SavedAt,
            CharacterCreated = CharacterCreated,
            CharacterName = CharacterName,
            CharacterPosition = new Vector2(CharacterPositionX, CharacterPositionY),
            CharacterDirection = CharacterDirection,
        };
    }
}
