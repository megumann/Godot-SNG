using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export]
	public int Speed { get; set; } = 200; // How fast the player will move in pixels/sec,
	
	public Vector2 ScreenSize; // Size of the game window.
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ScreenSize = GetViewportRect().Size;
		GD.Print("If you see this message, the player script is initialized!");
	}

	private string _lastDirection = "front"; // Default starting direction
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var velocity = Vector2.Zero; // The player's movement vector.
		
		if (Input.IsActionPressed("walk_right"))
		{
			velocity.X += 1;
			GD.Print("walk_right Pressed!");
		}
		
		if (Input.IsActionPressed("walk_left"))
		{
			velocity.X -= 1;
			GD.Print("walk_left Pressed!");
		}
		
		if (Input.IsActionPressed("walk_down"))
		{
			velocity.Y += 1;
			GD.Print("walk_down Pressed!");
		}
		
		if (Input.IsActionPressed("walk_up"))
		{
			velocity.Y -= 1;
			GD.Print("walk_up Pressed!");
		}
		
		var animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		
		if (velocity.X > 0)
		{
			animatedSprite2D.Animation = "walk_right";
			_lastDirection = "right";
		}
		if (velocity.X < 0)
		{
			animatedSprite2D.Animation = "walk_left";
			_lastDirection = "left";
		}
		if (velocity.Y < 0)
		{
			animatedSprite2D.Animation = "walk_back";
			_lastDirection = "back";
		}
		if (velocity.Y > 0)
		{
			animatedSprite2D.Animation = "walk_front";
			_lastDirection = "front";
		}
		
		if (velocity.Length() > 0)
		{
			velocity = velocity.Normalized() * Speed;
			Velocity = velocity;
			animatedSprite2D.Play();
		}
		else
		{
			Velocity = Vector2.Zero;
			animatedSprite2D.Animation = "idle_" + _lastDirection;
			animatedSprite2D.Stop();
		}
		MoveAndSlide();
	}
}
