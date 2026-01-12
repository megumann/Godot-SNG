using Godot;
using System;

public partial class Main : Node2D
{
	public void GameOver()
	{
		
	}
	public void NewGame()
	{
		var startPosition = GetNode<Marker2D>("StartPosition");
		// player.Start(startPosition.Position);
	}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		NewGame();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
