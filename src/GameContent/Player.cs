namespace Codartesien.Game.GameContent;

using Godot;

public partial class Player : CharacterBody2D
{
    public const string PLAYER_GROUP = "player";
        
    private const int WALK_SPEED = 200;

    #region Properties
    public string CharacterName
    {
        get => _characterName;
        set {
            _characterName = value;
            Refresh();
        }
    }
    
    public string Direction { get; set; } = "down";
    #endregion

    #region Fields
    private Label _characterNameLabel;
    
    private AnimatedSprite2D _characterSprite;
    
    private string _characterName = "Player";

    private string _action = "idle";
    #endregion

    public override void _Ready()
    {
        AddToGroup(PLAYER_GROUP);
        
        _characterNameLabel = GetNode<Label>("%CharacterNameLabel");
        _characterSprite = GetNode<AnimatedSprite2D>("%CharacterAnimatedSprite");
    }
    
    public override void _PhysicsProcess(double delta)
    {
        var velocity = Velocity;

        if (Input.IsActionPressed("ui_left")) {
            velocity.X = -WALK_SPEED;
            Direction = "side";
            _action = "walk";
            _characterSprite.FlipH = true;
        } else if (Input.IsActionPressed("ui_right")) {
            velocity.X = WALK_SPEED;
            Direction = "side";
            _action = "walk";
            _characterSprite.FlipH = false;
        } else {
            velocity.X = 0;
        }
        
        if (Input.IsActionPressed("ui_up")) {
            velocity.Y = -WALK_SPEED;
            _action = "walk";
            Direction = "up";
        } else if (Input.IsActionPressed("ui_down")) {
            velocity.Y = WALK_SPEED;
            _action = "walk";
            Direction = "down";
        } else {
            velocity.Y = 0;
        }

        if (Velocity == Vector2.Zero) {
            _action = "idle";
        }

        Velocity = velocity;
        
        MoveAndSlide();
    }
    
    public override void _Process(double delta)
    {
        _characterSprite.Play($"{_action}_{Direction}");
    }

    private void Refresh()
    {
        _characterNameLabel.Text = CharacterName;
    }
}
