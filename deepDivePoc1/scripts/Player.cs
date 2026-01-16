using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export]
	public int Speed { get; set; } = 100; // How fast the player will move in pixels/sec
	[Export]
	public int RunSpeed { get; set; } = 200; // How fast the player will move in pixels/sec when running
	[Export]
	public int ExhaustSpeed { get; set; } = 25; // Speed of the player after depleting stamina.

	// Stamina Variables
    [Export] public float MaxStamina { get; set; } = 100f;
    [Export] public float StaminaDrainRate { get; set; } = 30f; // Stamina lost per second while sprinting
    [Export] public float StaminaRegenRate { get; set; } = 15f; // Stamina regenerated per second
    [Export] public float ExhaustionDuration { get; set; } = 3f; // How long player stays exhausted before regen starts
    [Export] public float AttackStaminaCost { get; set; } = 20f; // Stamina cost per attack
    [Export] public float StaminaCooldown { get; set; } = 0.5f; // Cooldown before stamina can regen after an attack
    [Export] public float LowStaminaThreshold { get; set; } = 20f; // Stamina level to trigger low stamina warning
    [Export] public float FlashSpeed { get; set; } = 6f; // Speed of the flashing effect
    [Export] public float StaminaBarSmoothSpeed { get; set; } = 5f; // Speed of stamina bar smooth animation
    
    private float _stamina;
    private float _displayedStamina; // Smoothly animated stamina for display
    private bool _isExhausted = false;
    private float _exhaustionTimer = 0f; // Timer for exhaustion duration
    private float _staminaCooldownTimer = 0f; // Timer for stamina cooldown after attack
    private float _flashTimer = 0f; // Timer for low stamina flash effect
    private ProgressBar _staminaBar;

	public Vector2 ScreenSize; // Size of the game window.
	// Called when the node enters the scene tree for the first time.
	private string _lastDirection = "front"; // Default starting direction
	private bool _isAttacking = false; // New flag to track attack state
	public override void _Ready()
	{
		ScreenSize = GetViewportRect().Size;

		// Initialize Stamina
        _stamina = MaxStamina;
        _displayedStamina = MaxStamina;
        _staminaBar = GetNode<ProgressBar>("StaminaBar");
        _staminaBar.MaxValue = MaxStamina;
        _staminaBar.Value = _displayedStamina;
        _staminaBar.Hide(); // Hide initially

		GD.Print("If you see this message, the player script is initialized!");
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
        var velocity = Vector2.Zero; // The player's movement vector.
		var animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

		if (Input.IsActionJustPressed("attack") && !_isAttacking)
		{
			// Check if player has enough stamina to attack
			if (_stamina >= AttackStaminaCost)
			{
				_isAttacking = true;
				_stamina -= AttackStaminaCost;
				_staminaCooldownTimer = 0f; // Start cooldown after attack
				
				// Update stamina bar immediately to show attack cost
				if (_staminaBar != null)
				{
					_staminaBar.Value = _stamina;
					_staminaBar.Show();
				}
				
				// Check if attack depleted stamina to trigger exhaustion
				if (_stamina <= 0)
				{
					_stamina = 0;
					_isExhausted = true;
					_exhaustionTimer = 0f;
					GD.Print("Player exhausted from attack!");
				}
				
				string dir = _lastDirection;
				if (dir == "front") dir = "down";
				else if (dir == "back") dir = "up";

				string attackAnim = "attack_" + dir;
				animatedSprite2D.Animation = attackAnim;
				animatedSprite2D.Play();

				Velocity = Vector2.Zero; // Stop movement during attack	
				return; // Skip movement while attacking
			}
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

		// Check if player is moving
		bool isMoving = velocity.Length() > 0;
		
		// Update stamina BEFORE calculating speed to ensure exhaustion state is current
		bool canRun = Input.IsActionPressed("run") && !_isExhausted;
		UpdateStamina(canRun && isMoving, delta);
		
		// Determine which speed to use (after stamina update)
		bool isRunning = Input.IsActionPressed("run") && !_isExhausted;
		int currentSpeed = isRunning ? RunSpeed : (_isExhausted ? ExhaustSpeed : Speed);
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

	private void UpdateStamina(bool isRunning, double delta)
	{
		// Update stamina cooldown timer
		if (_staminaCooldownTimer > 0)
		{
			_staminaCooldownTimer -= (float)delta;
		}
		
		if (isRunning && _stamina > 0)
		{
			// Drain stamina while sprinting
			_stamina -= (float)(StaminaDrainRate * delta);
			
			if (_stamina <= 0)
			{
				_stamina = 0;
				_isExhausted = true;
				_exhaustionTimer = 0f;
				GD.Print("Player exhausted!");
			}
		}
		else if (_isExhausted)
		{
			// Count down exhaustion timer before regen starts
			_exhaustionTimer += (float)delta;
			
			if (_exhaustionTimer >= ExhaustionDuration)
			{
				// Start regenerating stamina (only if cooldown expired)
				if (_staminaCooldownTimer <= 0)
				{
					_stamina += (float)(StaminaRegenRate * delta);
					
					if (_stamina >= MaxStamina)
					{
						_stamina = MaxStamina;
						_isExhausted = false;
						GD.Print("Stamina fully recovered!");
					}
				}
			}
		}
		else if (_stamina < MaxStamina && _staminaCooldownTimer <= 0)
		{
			// Passive stamina regeneration when not sprinting (only if cooldown expired)
			_stamina += (float)(StaminaRegenRate * delta);
			
			if (_stamina > MaxStamina)
			{
				_stamina = MaxStamina;
			}
		}

		// Update the progress bar
		if (_staminaBar != null)
		{
			// Smoothly interpolate the displayed stamina towards the actual stamina
			if (Mathf.Abs(_displayedStamina - _stamina) > 0.1f)
			{
				_displayedStamina = Mathf.Lerp(_displayedStamina, _stamina, (float)(StaminaBarSmoothSpeed * delta));
			}
			else
			{
				_displayedStamina = _stamina; // Snap to actual value when very close
			}
			
			_staminaBar.Value = _displayedStamina;
			
			// Show bar when not at max stamina
			if (_stamina < MaxStamina)
			{
				_staminaBar.Show();
			}
			else if (!_isExhausted)
			{
				_staminaBar.Hide();
			}
			
			// Apply flashing effect when exhausted or stamina is low (until 20% full)
			float recoveryThreshold = MaxStamina * 0.2f;
			bool shouldFlash = _isExhausted || (_stamina <= LowStaminaThreshold && _stamina <= recoveryThreshold);
			
			if (shouldFlash)
			{
				_flashTimer += (float)delta;
				float flashValue = (float)Math.Sin(_flashTimer * FlashSpeed) * 0.5f + 0.5f; // Oscillate between 0 and 1
				_staminaBar.Modulate = new Color(1, 1, 1, flashValue); // Flash opacity
			}
			else
			{
				_flashTimer = 0f;
				_staminaBar.Modulate = Colors.White; // Reset to normal opacity
			}
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