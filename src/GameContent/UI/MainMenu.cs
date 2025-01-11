namespace Codartesien.Game.GameContent.UI;

using Godot;
using Systems.SaveGame;

/// <summary>
/// Main menu of the game (start menu), which can be displayed in game or before the game starts.
/// </summary>
public partial class MainMenu : Control
{
    #region Signals
    [Signal]
    public delegate void ContinueGameButtonPressedEventHandler();

    [Signal]
    public delegate void LoadButtonPressedEventHandler();

    [Signal]
    public delegate void NewGameButtonPressedEventHandler();

    [Signal]
    public delegate void ExitDesktopButtonPressedEventHandler();
    #endregion

    #region Fields
    private SaveGameManager _saveGameManager;

    private Button _continueButton;
    
    private Button _loadButton;

    private Button _newGameButton;

    private Button _exitButton;

    private Label _saveSlotInfoLabel;
    #endregion

    public new bool Visible
    {
        get => base.Visible;
        set {
            base.Visible = value;

            if (!value) {
                return;
            }
            GD.Print("Main menu UI opened");
            OnMadeVisible();
        }
    }


    public override void _Ready()
    {
        _saveGameManager = GetNode<SaveGameManager>("/root/SaveGameManager");
        _continueButton = GetNode<Button>("%ContinueButton");
        _loadButton = GetNode<Button>("%LoadButton");
        _newGameButton = GetNode<Button>("%NewGameButton");
        _exitButton = GetNode<Button>("%ExitButton");
        _saveSlotInfoLabel = GetNode<Label>("%SaveSlotInfoLabel");

        _continueButton.Visible = false;
        _loadButton.Visible = false;

        _newGameButton.Pressed += OnNewGameButtonPressed;
        _loadButton.Pressed += OnLoadButtonPressed;
        _exitButton.Pressed += OnExitButtonPressed;
        _continueButton.Pressed += OnContinueButtonPressed;

        _saveGameManager.SaveLoaded += OnSaveLoaded;
    }

    public override void _ExitTree()
    {
        _newGameButton.Pressed -= OnNewGameButtonPressed;
        _exitButton.Pressed -= OnExitButtonPressed;
        _continueButton.Pressed -= OnContinueButtonPressed;
        _saveGameManager.SaveLoaded -= OnSaveLoaded;
    }

    private void OnSaveLoaded(SaveGameDataResource saveGameData)
    {
        RefreshButtons();
    }

    private void OnMadeVisible()
    {
        _saveSlotInfoLabel.Text = _saveGameManager.CurrentSaveSlot?.Id > -1 ? $"Save Slot #{_saveGameManager.CurrentSaveSlot.Id} (Character: {_saveGameManager.CurrentSaveSlot.CharacterName})" : "No save slot selected";
        RefreshButtons();
    }

    private void RefreshButtons()
    {
        _continueButton.Visible = _saveGameManager.CurrentSaveSlot?.Exists ?? false;
        _loadButton.Visible = _continueButton.Visible;
    }

    private void OnContinueButtonPressed()
    {
        EmitSignal(SignalName.ContinueGameButtonPressed);
    }

    private void OnNewGameButtonPressed()
    {
        EmitSignal(SignalName.NewGameButtonPressed);
    }

    private void OnLoadButtonPressed()
    {
        EmitSignal(SignalName.LoadButtonPressed);
    }

    private void OnExitButtonPressed()
    {
        EmitSignal(SignalName.ExitDesktopButtonPressed);
    }
}
