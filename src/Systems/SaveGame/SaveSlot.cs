namespace Codartesien.Game.Systems.SaveGame;

public class SaveSlot
{
    public int Id { get; init; }

    public bool Exists { get; set; }

    public string SavedAt { get; set; }

    public string CharacterName { get; set; }
}
