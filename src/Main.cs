namespace Codartesien.Game;

using GameContent;
using GameContent.UI;
using Godot;
using Systems.SaveGame;

/// <summary>
/// Main scene of the game.
/// </summary>
public partial class Main : Node
{
    #region Fields
    private SaveGameManager _saveGameManager;

    private MainMenu _mainMenu;

    private SaveSlotSelectionMenu _saveSelectionMenu;
    
    private CreateCharacterScreen _createCharacterScreen;
    
    private GameWorld _gameWorld;
    
    private PackedScene _gameWorldScene = ResourceLoader.Load<PackedScene>("res://src/GameContent/GameWorld.tscn");
    #endregion

    public override void _Ready()
    {
        GD.Print("Main initializing");

        _saveGameManager = GetNode<SaveGameManager>("/root/SaveGameManager");
        _mainMenu = GetNode<MainMenu>("%MainMenu");
        _saveSelectionMenu = GetNode<SaveSlotSelectionMenu>("%SaveSlotSelectionMenu");
        _createCharacterScreen = GetNode<CreateCharacterScreen>("%CreateCharacterScreen");
        
        _mainMenu.ExitDesktopButtonPressed += OnQuitButtonPressed;
        _mainMenu.NewGameButtonPressed += OnNewGameButtonPressed;
        _mainMenu.ContinueGameButtonPressed += OnContinueButtonPressed;
        _mainMenu.LoadButtonPressed += OnLoadButtonPressed;
        _saveSelectionMenu.SaveSlotSelected += OnSaveSelected;
        _saveSelectionMenu.BackButtonPressed += OnSaveSelectionBackButtonPressed;
        _createCharacterScreen.ConfirmButtonPressed += OnCharacterCreated;
        _saveGameManager.SaveLoaded += OnSaveLoaded;

        _saveGameManager.LoadLastSave(true);

        OpenMainMenu();
    }

    private void OpenMainMenu()
    {
        GetTree().Paused = true;
        
        HideAllMenus();
        _mainMenu.Visible = true;
    }

    private void HideAllMenus()
    {
        _mainMenu.Visible = false;
        _saveSelectionMenu.Visible = false;
        _createCharacterScreen.Visible = false;
    }
    
    private void OnContinueButtonPressed()
    {
        // If the game world already, exists, we are just resuming the game
        if (IsInstanceValid(_gameWorld)) {
            StartGame(null);
        }
        
        // If the world does not exist yet, 
        StartGame(_saveGameManager.LoadedSaveGameData);
    }

    private void StartGame(SaveGameDataResource saveGameData)
    {
        HideAllMenus();

        if (!IsInstanceValid(_gameWorld)) {
            _gameWorld = _gameWorldScene.Instantiate<GameWorld>();
            _gameWorld.ExitGameButtonPressed += OpenMainMenu;
            AddChild(_gameWorld);
        }
        
        _gameWorld.Initialize(saveGameData);

        GetTree().Paused = false;
    }

    private void OpenSaveSelectionMenu(SaveSlotSelectionMenu.SlotSelectionMode mode)
    {
        HideAllMenus();
        
        _saveSelectionMenu.Mode = mode;
        _saveSelectionMenu.Visible = true;
    }

    private void OnNewGameButtonPressed()
    {
        if (_saveGameManager.HasSaves()) {
            OpenSaveSelectionMenu(SaveSlotSelectionMenu.SlotSelectionMode.NewGame);
        } else {
            // No existing saves, start a new game on the first save slot
            _createCharacterScreen.SaveSlotId = 0;
            _createCharacterScreen.Visible = true;
        }
    }

    private void OnLoadButtonPressed()
    {
        OpenSaveSelectionMenu(SaveSlotSelectionMenu.SlotSelectionMode.LoadSlot);
    }
    
    private void OnQuitButtonPressed()
    {
        GD.Print("Exiting game");
        GetTree().CallDeferred(SceneTree.MethodName.Quit);
    }

    private void OnSaveSelectionBackButtonPressed()
    {
        HideAllMenus();
        _mainMenu.Visible = true;
    }

    private void OnSaveSelected(int saveSlotId)
    {
        HideAllMenus();
        
        if (_saveSelectionMenu.Mode == SaveSlotSelectionMenu.SlotSelectionMode.LoadSlot) {
            // Load the given save slot
            _saveGameManager.Load(saveSlotId);
        } else {
            // Prepare a new game on the given slot
            _createCharacterScreen.SaveSlotId = saveSlotId;
            _createCharacterScreen.Visible = true;
        }
    }
    
    private void OnCharacterCreated(int saveSlotId, string characterName)
    {
        // Create a new save on the given slot
        _saveGameManager.CreateSave(saveSlotId, characterName);
        
        // Load it to start the game
        _saveGameManager.Load(saveSlotId);
    }
    
    private void OnSaveLoaded(SaveGameDataResource saveGameData)
    {
        // If the game world node exists, remove it
        _gameWorld?.QueueFree();
        _gameWorld = null;
        
        CallDeferred(MethodName.StartGame, saveGameData);
    }
}
