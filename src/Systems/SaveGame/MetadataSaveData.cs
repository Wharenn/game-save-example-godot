namespace Codartesien.Game.Systems.SaveGame;

/// <summary>
/// Lightweight class to read the metadata of any save data by deserialization. 
/// </summary>
public class MetadataSaveData
{
    public int SaveVersion { get; init; }

    public string SavedAt { get; init; }
}
