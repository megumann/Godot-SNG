using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export]
	public int Speed { get; set; } = 100; // How fast the player will move in pixels/sec
	[Export]
	public int runSpeed { get; set; } = 200; // How fast the player will move in pixels/sec when running
	
	
	public Vector2 ScreenSize; // Size of the game window.
	// Called when the node enters the scene tree for the first time.
	private string _lastDirection = "front"; // Default starting direction
	private bool _isAttacking = false; // New flag to track attack state
	public override void _Ready()
	{
		ScreenSize = GetViewportRect().Size;
		GD.Print("If you see this message, the player script is initialized!");
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
        var velocity = Vector2.Zero; // The player's movement vector.
		var animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

		if (Input.IsActionJustPressed("attack") && !_isAttacking)
		{
			_isAttacking = true;
			string dir = _lastDirection;
			if (dir == "front") dir = "down";
			else if (dir == "back") dir = "up";

			string attackAnim = "attack_" + dir;
			animatedSprite2D.Animation = attackAnim;
			animatedSprite2D.Play();

			Velocity = Vector2.Zero; // Stop movement during attack	
			return; // Skip movement while attacking
		}

		if (_isAttacking)
		{
			return;
		}
		
		// Gather input
		if (Input.IsActionPressed("walk_right")) velocity.X += 1;
		if (Input.IsActionPressed("walk_left")) velocity.X -= 1;
		if (Input.IsActionPressed("walk_down")) velocity.Y += 1;
		if (Input.IsActionPressed("walk_up")) velocity.Y -= 1;

		// Determine which speed to use
		bool isRunning = Input.IsActionPressed("run");
		int currentSpeed = Input.IsActionPressed("run") ? runSpeed : Speed;
		string animPrefix = isRunning ? "run_" : "walk_";
		
		// Define direction based on final velocity
		if (velocity.X > 0) _lastDirection = "right";
		else if (velocity.X < 0) _lastDirection = "left";
		else if (velocity.Y > 0) _lastDirection = "front";
		else if (velocity.Y < 0) _lastDirection = "back";
		
		// Apply movement OR idle
		if (velocity.Length() > 0)
		{
			Velocity = velocity.Normalized() * currentSpeed;
			
			// Use saved direction to pick the walk animation to handle naming inconsistencies
			if (_lastDirection == "front") animatedSprite2D.Animation = animPrefix + "down";
			else if (_lastDirection == "back") animatedSprite2D.Animation = animPrefix + "up";
			else animatedSprite2D.Animation = animPrefix + _lastDirection;

			/*if (_lastDirection == "front") animatedSprite2D.Animation = "walk_down";
			else if (_lastDirection == "back") animatedSprite2D.Animation = "walk_up";
			else animatedSprite2D.Animation = "walk_" + _lastDirection;*/
			
			animatedSprite2D.Play();
		}
		else
		{
			// Only runs if NOT moving
			Velocity = Vector2.Zero;
			string newIdle = "idle_" + _lastDirection;
			
			if (animatedSprite2D.Animation != newIdle)
			{
				animatedSprite2D.Animation = newIdle;
				GD.Print(newIdle + " attempted!");
				
				animatedSprite2D.Play();
			}
		}
		MoveAndSlide();
	}

	private void OnAnimationFinished()
	{
		if (GetNode<AnimatedSprite2D>("AnimatedSprite2D").Animation.ToString().StartsWith("attack_", StringComparison.Ordinal))
		{
			_isAttacking = false;
		}
	}
}

/*
Animation Naming Conventions:
- Walking Animations:
  - walk_up
  - walk_down
  - walk_left
  - walk_right
- Running Animations:
  - run_up
  - run_down
  - run_left
  - run_right
- Idle Animations:
  - idle_up
  - idle_down
  - idle_left
  - idle_right
- Attack Animations:
  - attack_up
  - attack_down
  - attack_left
  - attack_right

To Do:
- Implement health and damage system (not started, low priority)
- Add sound effects for walking, running, and attacking (not started, low priority)
- Implement collision detection with environment (not started, medium priority)
- Implement animations for taking damage and dying (not started, low priority)
- Create death flags and respawn mechanics (not started, low priority)
- Create main menu and pause menu (not started, low priority)
- Add collectible items and inventory system (not started, medium priority)
- Create multiple stages/levels for demonstration purposes (not started, medium priority)
- Add background music and ambient sounds (not started, low priority)
- Further polish (not started, low priority)
*/