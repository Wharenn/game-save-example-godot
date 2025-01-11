namespace Codartesien.Game.GameContent.UI;

using Godot;

public partial class CreateCharacterScreen : Control
{
    #region Signals
    [Signal]
    public delegate void ConfirmButtonPressedEventHandler(int saveSlotId, string characterName);
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
            
            OnMadeVisible();
        }
    }
    
    public int SaveSlotId { get; set; }
    #endregion

    #region Fields
    private LineEdit _lineEdit;

    private Button _confirmButton;
    #endregion

    public override void _Ready()
    {
        Visible = false;

        _lineEdit = GetNode<LineEdit>("%LineEdit");
        _confirmButton = GetNode<Button>("%ConfirmButton");
        _confirmButton.Pressed += OnConfirmButtonPressed;
    }

    public override void _ExitTree()
    {
        _confirmButton.Pressed -= OnConfirmButtonPressed;
    }
    
    private void OnConfirmButtonPressed()
    {
        Visible = false;
        EmitSignal(SignalName.ConfirmButtonPressed, SaveSlotId, _lineEdit.Text);
    }
    
    private void OnMadeVisible()
    {
        _lineEdit.Text = string.Empty;
        _lineEdit.GrabFocus();
    }
}
