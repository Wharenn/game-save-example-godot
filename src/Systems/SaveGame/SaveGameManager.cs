namespace Codartesien.Game.Systems.SaveGame;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using GameContent;
using Godot;

/// <summary>
/// Manages the saving and loading of the game data.
/// </summary>
public partial class SaveGameManager : Node
{
    private const int SAVE_VERSION = 1; // Increment this number when the save game format changes

    private const int SAVE_SLOT_COUNT = 3;

    private List<SaveSlot> _saveSlots;

    private int _currentSaveSlotId = -1;

    #region Signals
    [Signal]
    public delegate void SaveLoadedEventHandler(SaveGameDataResource saveGameData);
    #endregion

    #region Properties
    public List<SaveSlot> SaveSlots
    {
        get {
            if (_saveSlots == null) {
                ReadSaveSlots();
            }

            return _saveSlots;
        }
    }
    
    public SaveSlot CurrentSaveSlot => _currentSaveSlotId > -1 ? SaveSlots[_currentSaveSlotId] : null;
    
    public SaveGameDataResource LoadedSaveGameData { get; private set; }
    #endregion

    /// <summary>
    /// Create a new save file with the default content on the given slot.
    /// </summary>
    public void CreateSave(int identifier, string characterName)
    {
        var json = JsonSerializer.Serialize(BuildEmptySaveGameData(characterName), GetJsonSerializerOptions());

        using var file = FileAccess.Open(GetSaveGameFilePath(identifier), FileAccess.ModeFlags.Write);
        file.StoreLine(json);
        file.Flush();

        GD.Print($"Created save game to {GetSaveGameFilePath(identifier)}");
        
        _currentSaveSlotId = identifier;

        // Refresh the save slots after saving
        ReadSaveSlots();
    }

    public void Save()
    {
        GD.Print($"Saving game on slot {_currentSaveSlotId}");
        Save(_currentSaveSlotId);
    }

    /// <summary>
    /// Save the current game progression to the given save file.
    /// </summary>
    public void Save(int identifier)
    {
        var json = JsonSerializer.Serialize(BuildSaveGameData(), GetJsonSerializerOptions());

        using var file = FileAccess.Open(GetSaveGameFilePath(identifier), FileAccess.ModeFlags.Write);
        file.StoreLine(json);
        file.Flush();

        GD.Print($"Save game saved to {GetSaveGameFilePath(identifier)}");

        // Refresh the save slots after saving
        ReadSaveSlots();
    }

    /// <summary>
    /// Load the last save file available.
    /// The silent parameter is used to prevent the signal from being emitted.
    /// </summary>
    public bool LoadLastSave(bool silent = false)
    {
        // Find the slot that have the latest SavedAt date and which exists
        var existingSlots = SaveSlots.Where(slot => slot.Exists && !string.IsNullOrEmpty(slot.SavedAt)).ToList();
        existingSlots.Sort((a, b) => DateTime.Compare(DateTime.Parse(b.SavedAt), DateTime.Parse(a.SavedAt)));

        if (existingSlots.Count <= 0) {
            return false;
        }
        
        Load(existingSlots[0].Id, silent);

        return true;
    }
    
    /// <summary>
    /// Load the game progression from the given save file.
    /// The silent parameter is used to prevent the signal from being emitted.
    /// </summary>
    public void Load(int identifier, bool silent = false)
    {
        if (!FileAccess.FileExists(GetSaveGameFilePath(identifier))) {
            return;
        }

        using var saveFile = FileAccess.Open(GetSaveGameFilePath(identifier), FileAccess.ModeFlags.Read);
        GD.Print($"Loading save game from {GetSaveGameFilePath(identifier)}");

        var fileContent = saveFile.GetAsText();

        // Start by reading the version data to determine how to load the save game
        // for that, we use a light version class that only contains the version number
        var versionData = JsonSerializer.Deserialize<MetadataSaveData>(fileContent);
        GD.Print($"Identified Save Version #{versionData.SaveVersion}");

        var loadedData = ReadSaveGameData(versionData.SaveVersion, fileContent);
        if (loadedData == null) {
            GD.Print("Failed to load save game");
            return;
        }

        LoadedSaveGameData = loadedData.ToResource();
        _currentSaveSlotId = identifier;

        GD.Print("Save loaded with success");
        
        if (!silent) {
            EmitSignal(SignalName.SaveLoaded, LoadedSaveGameData);
        }
    }

    /// <summary>
    /// Returns true if there are savegame files available.
    /// </summary>
    public bool HasSaves()
    {
        return SaveSlots.Exists(slot => slot.Exists);
    }

    /// <summary>
    /// Create and return the save game data object to be saved in a file.
    /// </summary>
    private SaveGameData BuildSaveGameData()
    { 
        var player = GetTree().GetNodesInGroup(Player.PLAYER_GROUP).FirstOrDefault() as Player;
        
        return new SaveGameData {
            SaveVersion = SAVE_VERSION,
            SavedAt = DateTime.UtcNow.ToString("o"),
            CharacterName = player?.CharacterName ?? CurrentSaveSlot.CharacterName,
            CharacterPositionX = player?.GlobalPosition.X ?? 0,
            CharacterPositionY = player?.GlobalPosition.Y ?? 0,
            CharacterDirection = player?.Direction ?? "down"
        };
    }

    /// <summary>
    /// Create and return an empty save game data object, used for creating new save slots.
    /// </summary>
    private static SaveGameData BuildEmptySaveGameData(string characterName)
    {
        return new SaveGameData {
            SaveVersion = SAVE_VERSION,
            SavedAt = DateTime.UtcNow.ToString("o"),
            CharacterName = characterName,
            CharacterPositionX = 640,
            CharacterPositionY = 360,
        };
    }

    /// <summary>
    /// Load the save game data from the given file content at a specific version.
    /// This method job is to build the current format of the SaveGameData object with compatibility for older versions.
    /// </summary>
    private SaveGameData ReadSaveGameData(int version, string fileContent)
    {
        SaveGameData data = null;
        if (version == SAVE_VERSION) {
            // If the save game version is the current version, we can directly deserialize the data
            data = JsonSerializer.Deserialize<SaveGameData>(fileContent);
            // Use this code snippet to handle the case where the save game version is an older version:
            // } else if (version == 99) {
            //     var oldData = JsonSerializer.Deserialize<SaveGameDataVersion99>(fileContent);
            //     data = new SaveGameData {
            //         SaveVersion = SAVE_VERSION,
            //         UnlockedLevels = oldData.UnlockedLevels,
            //         SavedAt = oldData.SavedAt
            //     };
        } else {
            GD.Print($"Save game version {version} is not supported");
        }

        return data;
    }

    private static JsonSerializerOptions GetJsonSerializerOptions()
    {
        var options = new JsonSerializerOptions {
            IncludeFields = true,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
            WriteIndented = true
        };

        return options;
    }

    private static string GetSaveGameFilePath(int identifier)
    {
        return $"user://savegame_{identifier.ToString()}.save";
    }

    /// <summary>
    /// Read save files to extract save slots information.
    /// </summary>
    private void ReadSaveSlots()
    {
        _saveSlots ??= new List<SaveSlot>();

        for (var i = 0; i < SAVE_SLOT_COUNT; i++) {
            // Initialize the slot object or reuse the existing one
            var path = GetSaveGameFilePath(i);
            var slot = _saveSlots.Count <= i
                ? new SaveSlot {
                    Id = i
                }
                : _saveSlots[i];

            if (_saveSlots.Count <= i) {
                _saveSlots.Add(slot);
            }

            // Recheck every time if the file exists in case the slot was created or file was deleted
            slot.Exists = FileAccess.FileExists(path);
            if (!slot.Exists) {
                continue;
            }

            // Read the save fil content
            using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
            var fileContent = file.GetAsText();
            
            // Extract metadata information
            var versionData = JsonSerializer.Deserialize<MetadataSaveData>(fileContent);
            slot.SavedAt = versionData.SavedAt;
            
            // Load full save game data
            var loadedData = ReadSaveGameData(versionData.SaveVersion, fileContent);
            if (loadedData == null) {
                // If the save game data is invalid, skip
                continue;
            }

            // Populate the slot with relevant data
            slot.CharacterName = loadedData.CharacterName;
        }
    }
}
