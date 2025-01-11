namespace Codartesien.Game.GameContent.UI;

using Godot;

public partial class ConfirmScreen : Control
{
    #region Signals
    [Signal]
    public delegate void ConfirmButtonPressedEventHandler();

    [Signal]
    public delegate void CancelButtonPressedEventHandler();
    #endregion

    #region Fields
    private Button _cancelButtonNode;

    private Button _confirmButtonNode;
    #endregion

    public override void _Ready()
    {
        Visible = false;

        _confirmButtonNode = GetNode<Button>("%ConfirmButton");
        _cancelButtonNode = GetNode<Button>("%CancelButton");

        _cancelButtonNode.Pressed += OnCancelButtonPressed;
        _confirmButtonNode.Pressed += OnConfirmButtonPressed;
    }

    public override void _ExitTree()
    {
        _cancelButtonNode.Pressed -= OnCancelButtonPressed;
        _confirmButtonNode.Pressed -= OnConfirmButtonPressed;
    }

    private void OnConfirmButtonPressed()
    {
        Visible = false;
        EmitSignal(SignalName.ConfirmButtonPressed);
    }

    private void OnCancelButtonPressed()
    {
        Visible = false;
        EmitSignal(SignalName.CancelButtonPressed);
    }
}
