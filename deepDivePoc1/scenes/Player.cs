using Godot;
using System;

public partial class Player : Area2D
{
	[Export]
	public int Speed { get; set; } = 400; // How fast the player will move in pixels/sec,
	
	public Vector2 ScreenSize; // Size of the game window.
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ScreenSize = GetViewportRect().Size;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var velocity = Vector2.Zero; // The player's movement vector.
		
		if (Input.IsActionPressed("walk_right"))
		{
			velocity.X += 1;
		}
		
		if (Input.IsActionPressed("walk_left"))
		{
			velocity.X -= 1;
		}
		
		if (Input.IsActionPressed("walk_down"))
		{
			velocity.Y += 1;
		}
		
		if (Input.IsActionPressed("walk_up"))
		{
			velocity.Y -= 1;
		}
		
		var animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		
		if (velocity.Length() > 0)
		{
			velocity = velocity.Normalized() * Speed;
			animatedSprite2D.Play();
		}
		else
		{
			animatedSprite2D.Stop();
		}
	}
}
