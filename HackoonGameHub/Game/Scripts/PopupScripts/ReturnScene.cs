using Godot;
using System;

public partial class ReturnScene : Window
{

	public Label subtitleLabel;
	public Label descriptionLabel;
	public override void _Ready()
	{

		CloseRequested += OnWindowCloseRequest;
		
		subtitleLabel = GetNode<Label>("PanelContainer/VBoxContainer/subtitleLabel");
		descriptionLabel = GetNode<Label>("PanelContainer/VBoxContainer/descriptionLabel");
		
		Hide();
	}

	private void OnWindowCloseRequest()
	{
		Hide();
		
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			CloseRequested -= OnWindowCloseRequest;
		}
		base.Dispose(disposing);
	}
}
