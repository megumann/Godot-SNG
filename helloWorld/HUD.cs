using Godot;
using System;

public partial class HUD : CanvasLayer
{
	[Signal]
	public delegate void StartGameEventHandler();
	
	public void ShowMessage(string text)
	{
		var message = GetNode<Label>("Message");
		message.Text = text;
		message.Show();
		
		GetNode<Timer>("MessageTimer").Start();
	}
	
	async public void ShowGameOver()
	{
		ShowMessage("Game Over");
		
		var messageTimer = GetNode<Timer>("MessageTimer");
		await ToSignal(messageTimer, Timer.SignalName.Timeout);
		
		ShowMessage("Try Again?");
		//var message = GetNode<Label>("Message");
		//message.Text = "Try Again?";
		//message.Show();
		
		await ToSignal(GetTree().CreateTimer(1.0), SceneTreeTimer.SignalName.Timeout);
		GetNode<HBoxContainer>("MainMenu").Show();
		// GetNode<Button>("MainMenu/StartButton").Show();
		// GetNode<Button>("MainMenu/StopButton").Show();
		
	}
	
	public void UpdateScore(int score)
	{
		GetNode<Label>("ScoreLabel").Text = score.ToString();
	}
	
	private void OnStartButtonPressed()
	{
		GetNode<Button>("MainMenu/StartButton");
		var message = GetNode<Label>("Message");
		GD.Print("Start Button Pressed");
		ShowMessage("Dodge the Creeps!");
		GetNode<HBoxContainer>("MainMenu").Hide();
		// GetNode<Button>("MainMenu/StartButton").Hide();
		// GetNode<Button>("MainMenu/StopButton").Hide();
		EmitSignal(SignalName.StartGame);
		message.Hide();
	}
	
	private void OnStopButtonPressed()
	{
		GetNode<Button>("MainMenu/StopButton");
		var message = GetNode<Label>("Message");
		ShowMessage("Thanks for Playing!");
		GD.Print("Stop Button Pressed");
		//GetNode<Button>("StopButton").Hide();
		//GetNode<Button>("StartButton").Hide();
		
		
		GetTree().Quit();
	}
	
	private void OnMessageTimerTimeout()
	{
		GetNode<Label>("Message").Hide();
	}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
