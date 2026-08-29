using Godot;

public partial class Personagem : AnimatedSprite2D
{
	private const float Speed = 100.0f;
	
	[Export]public Vector2 SyncDirection = Vector2.Zero;
	[Export]public string SyncAnimation = "idleDown";

	public override void _Ready()
	{	
		GD.Print($"[READY] Boneco: {Name} | Minha ID Local: {Multiplayer.GetUniqueId()} | Sou o Dono? {IsMultiplayerAuthority()}");
	}

	public override void _PhysicsProcess(double delta)
	{
		if (IsMultiplayerAuthority())
		{
			Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
			SyncDirection = direction;
			
			AnimationRender(direction);
		}
		GlobalPosition += SyncDirection * Speed * (float)delta;
		if (Animation != SyncAnimation) Play(SyncAnimation);
	}

	private void AnimationRender(Vector2 directon)
	{
		if (directon.Length() > 0)
		{
			if (directon.X < 0 && directon.Y > 0) SyncAnimation = "downLeft";
			else if (directon.X > 0 && directon.Y > 0) SyncAnimation = "downRight";
			else if (directon.X < 0 && directon.Y < 0) SyncAnimation = "topLeft";
			else if (directon.X > 0 && directon.Y < 0) SyncAnimation = "topRight";
		
			else if (directon.Equals(Vector2.Left)) SyncAnimation = "left";
			else if (directon.Equals(Vector2.Right)) SyncAnimation = "right";
			else if (directon.Equals(Vector2.Up)) SyncAnimation = "up";
			else if (directon.Equals(Vector2.Down)) SyncAnimation = "down";
		}else
		{
			if (SyncAnimation == "left") SyncAnimation = "idleLeft";
			else if (SyncAnimation == "right") SyncAnimation = "idleRight";
			else if (SyncAnimation == "up") SyncAnimation = "idleUp";
			else if (SyncAnimation == "down") SyncAnimation = "idleDown";
			else if (SyncAnimation == "topLeft") SyncAnimation = "idleTopLeft";
			else if (SyncAnimation == "topRight") SyncAnimation = "idleTopRight";
			else if (SyncAnimation == "downLeft") SyncAnimation = "idleDownLeft";
			else if (SyncAnimation == "downRight") SyncAnimation = "idleDownRight";
		}
	}
}
