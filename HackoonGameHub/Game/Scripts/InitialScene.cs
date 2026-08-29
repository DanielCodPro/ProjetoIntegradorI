using Godot;
using System;
using Service;


public partial class InitialScene : Control
{
	private Button btnStart;
	private Button btnEnter;
	
	public override void _Ready()
	{
		btnStart = GetNode<Button>("PanelContainer/VBoxContainer/StartButton");
		btnEnter = GetNode<Button>("PanelContainer/VBoxContainer/EnterButton");
		
		btnStart.Pressed += OnBtnStartPressed;
		btnEnter.Pressed += OnBtnEnterPressed;
	}

	private void OnBtnStartPressed()
	{
		
		Error result = GetTree().ChangeSceneToFile("res://Scenes/Forms/FormsProfessor.tscn");

		if (result != Error.Ok)
		{
			GD.Print("Erro ao carrergr a cena: " + result);
		}
	}

	private void OnBtnEnterPressed()
	{
		Error result = GetTree().ChangeSceneToFile("res://Scenes/AlunoEntrar.tscn");
		
		if (result != Error.Ok)
		{
			GD.Print("Erro ao carregar a cena: " + result);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
}
