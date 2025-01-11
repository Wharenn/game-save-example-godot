namespace Codartesien.Game.GameContent.UI;

using System.Collections.Generic;
using Godot;
using Systems.SaveGame;

/// <summary>
/// A menu allowing the player to select a save slot.
/// </summary>
public partial class SaveSlotSelectionMenu : Control
{
    public enum SlotSelectionMode
    {
        LoadSlot,
        NewGame
    }

    #region Signals
    [Signal]
    public delegate void BackButtonPressedEventHandler();

    [Signal]
    public delegate void SaveSlotSelectedEventHandler(int saveSlotIdentifier);
    #endregion

    #region Properties
    public new bool Visible
    {
        get => base.Visible;
        set {
            base.Visible = value;
            
            if (!value) {
                return;
            }
            
            GD.Print("Save slot selection menu UI opened");
            OnMadeVisible();
        }
    }

    public SlotSelectionMode Mode { get; set; } = SlotSelectionMode.LoadSlot;

    [Export]
    public PackedScene SaveSlotGridItemScene { get; set; }
    #endregion

    #region Fields
    private SaveGameManager _saveGameManager;

    private GridContainer _gridContainer;

    private ConfirmScreen _confirmScreen;
    
    private Button _backButton;

    private int _confirmSaveSlotId = -1;
    #endregion


    public override void _Ready()
    {
        _saveGameManager = GetNode<SaveGameManager>("/root/SaveGameManager");
        _gridContainer = GetNode<GridContainer>("%GridContainer");
        _confirmScreen = GetNode<ConfirmScreen>("%ConfirmScreen");
        _backButton = GetNode<Button>("%BackButton");

        // Clear all existing grid items
        foreach (var child in _gridContainer.GetChildren()) {
            child.QueueFree();
        }

        _confirmScreen.ConfirmButtonPressed += OnConfirmScreenConfirmButtonPressed;
        _confirmScreen.CancelButtonPressed += OnConfirmScreenCancelButtonPressed;
        
        _backButton.Pressed += OnBackActionPressed;
    }

    public override void _ExitTree()
    {
        _confirmScreen.ConfirmButtonPressed -= OnConfirmScreenConfirmButtonPressed;
        _confirmScreen.CancelButtonPressed -= OnConfirmScreenCancelButtonPressed;
    }

    private void OnMadeVisible()
    {
        List<SaveSlot> existingSaveSlots = new();
        List<SaveSlotGridItem> gridItems = new();

        foreach (var child in _gridContainer.GetChildren()) {
            if (child is SaveSlotGridItem saveSlotGridItem) {
                gridItems.Add(saveSlotGridItem);
                existingSaveSlots.Add(saveSlotGridItem.SaveSlot);
            }
        }

        var saveSlots = _saveGameManager.SaveSlots;
        foreach (var saveSlot in saveSlots) {
            SaveSlotGridItem saveSlotGridItem;
            if (!existingSaveSlots.Contains(saveSlot)) {
                saveSlotGridItem = SaveSlotGridItemScene.Instantiate<SaveSlotGridItem>();
                saveSlotGridItem.SaveSlot = saveSlot;
                saveSlotGridItem.SaveSlotSelected += OnSaveSlotSelected;
                _gridContainer.AddChild(saveSlotGridItem);
            } else {
                saveSlotGridItem = gridItems.Find(i => i.SaveSlot == saveSlot);
                saveSlotGridItem.SaveSlot = saveSlot;
            }
            
            if (Mode == SlotSelectionMode.LoadSlot && !saveSlot.Exists) {
                saveSlotGridItem.Enabled = false;
            } else {
                saveSlotGridItem.Enabled = true;
            }
        }
    }

    private void OnBackActionPressed()
    {
        EmitSignal(SignalName.BackButtonPressed);
    }

    private void OnSaveSlotSelected(int saveSlotId)
    {
        if (Mode == SlotSelectionMode.LoadSlot && !_saveGameManager.SaveSlots[saveSlotId].Exists) {
            // If slot selection mode, do nothing if we select a free slot
            return;
        }
        if (Mode == SlotSelectionMode.NewGame && _saveGameManager.SaveSlots[saveSlotId].Exists) {
            // If new game selection mode, ask for confirmation if the slot is already used
            _confirmSaveSlotId = saveSlotId;
            _confirmScreen.Visible = true;
            return;
        }

        GD.Print($"Selected save slot internal id: {saveSlotId.ToString()}");
        EmitSignal(SignalName.SaveSlotSelected, saveSlotId);
    }

    private void OnConfirmScreenConfirmButtonPressed()
    {
        _confirmScreen.Visible = false;

        GD.Print($"Selected save and confirmed override of slot internal id: {_confirmSaveSlotId.ToString()}");
        EmitSignal(SignalName.SaveSlotSelected, _confirmSaveSlotId);

        // Reset value for next confirmation
        _confirmSaveSlotId = -1;
    }

    private void OnConfirmScreenCancelButtonPressed()
    {
        // Reset value for next confirmation
        _confirmSaveSlotId = -1;
    }
}
