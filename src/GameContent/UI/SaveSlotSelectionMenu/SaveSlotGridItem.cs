namespace Codartesien.Game.GameContent.UI;

using System;
using Godot;
using Systems.SaveGame;

/// <summary>
/// A grid item representing a save slot.
/// </summary>
public partial class SaveSlotGridItem : PanelContainer
{
    #region Signals
    [Signal]
    public delegate void SaveSlotSelectedEventHandler(int saveSlotId);
    #endregion

    #region Properties
    public SaveSlot SaveSlot
    {
        get => _saveSlot;
        set {
            _saveSlot = value;
            Refresh();
        }
    }

    public bool Enabled
    {
        get => _enabled;
        set {
            _enabled = value;
            Refresh();
        }
    }
    #endregion

    #region Fields
    private Button _selectButton;
    
    private Label _nameLabel;

    private Label _savedAtLabel;

    private SaveSlot _saveSlot;
    
    private bool _enabled;
    #endregion

    public override void _Ready()
    {
        _selectButton = GetNode<Button>("%SelectButton");
        _nameLabel = GetNode<Label>("%NameLabel");
        _savedAtLabel = GetNode<Label>("%SavedAtLabel");
        
        _selectButton.Pressed += OnSelectButtonPressed;

        Refresh();
    }

    private void Refresh()
    {
        if (_saveSlot == null || !IsInstanceValid(_nameLabel)) {
            return;
        }

        if (_saveSlot.Exists) {
            // Convert string YYYY-MM-DD HH:MM:SS to a more readable format
            var dateTime = DateTime.Parse(_saveSlot.SavedAt);
            _savedAtLabel.Text = dateTime.ToString("g");
            _nameLabel.Text = _saveSlot.CharacterName;
        } else {
            _nameLabel.Text = "Free Slot";
            _savedAtLabel.Text = "";
            _selectButton.Visible = Enabled;
        }
    }

    private void OnSelectButtonPressed()
    {
        EmitSignal(SignalName.SaveSlotSelected, SaveSlot.Id);
    }
}
