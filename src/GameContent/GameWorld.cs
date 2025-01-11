namespace Codartesien.Game.GameContent;

using Godot;
using Systems.SaveGame;

public partial class GameWorld : Node2D
{
    #region Signals
    [Signal]
    public delegate void ExitGameButtonPressedEventHandler();
    #endregion

    #region Fields
    private Player _player;
    
    private SaveGameManager _saveGameManager;

    private Button _saveButton;

    private Button _loadButton;
    
    private Button _menuButton;
    #endregion
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Pausable;
        
        _saveGameManager = GetNode<SaveGameManager>("/root/SaveGameManager");
        _saveButton = GetNode<Button>("%SaveButton");
        _loadButton = GetNode<Button>("%LoadButton");
        _menuButton = GetNode<Button>("%MenuButton");
        
        _saveButton.Pressed += OnSaveButtonPressed;
        _loadButton.Pressed += OnLoadButtonPressed;
        _menuButton.Pressed += OnMenuButtonPressed;
        
        _player = GetNode<Player>("%Player");
        _player.CharacterName = _saveGameManager.CurrentSaveSlot.CharacterName;
    }

    public override void _Input(InputEvent @event)
    {
        if (Input.IsActionJustPressed("ui_cancel")) {
            // Open main menu when pressing the escape button key
            GetViewport().SetInputAsHandled();
            EmitSignal(SignalName.ExitGameButtonPressed);
        }
    }
    
    private void OnSaveButtonPressed()
    {
        _saveGameManager.Save();
    }
    
    private void OnLoadButtonPressed()
    {
        _saveGameManager.Load(_saveGameManager.CurrentSaveSlot.Id);
    }
    
    private void OnMenuButtonPressed()
    {
        EmitSignal(SignalName.ExitGameButtonPressed);
    }
    
    public void Initialize(SaveGameDataResource saveGameData)
    {
        if (saveGameData == null) {
            return;
        }
        
        _player.CharacterName = saveGameData.CharacterName;
        _player.GlobalPosition = saveGameData.CharacterPosition;
        _player.Direction = saveGameData.CharacterDirection;
    }
}
