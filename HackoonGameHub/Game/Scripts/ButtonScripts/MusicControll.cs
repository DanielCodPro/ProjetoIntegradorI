using Godot;
using System;

public partial class MusicControll : Control
{
	[Export] public Texture2D music;
	[Export] public Texture2D noMusic;

	
	private Button btnMusic;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		btnMusic = GetNode<Button>("MusicButton");

		btnMusic.Pressed += OnBtnMusicPressed;
		
	}
	
	private void OnBtnMusicPressed()
	{
		if (music == null || noMusic == null)
		{
			GD.PrintErr("Faltam os ícones no Inspector!");
			return;
		}

		MusicScene.Tocando = !MusicScene.Tocando;
		MusicScene.sinal = true;
	}
	public override void _Process(double delta)
	{
		if (MusicScene.Tocando)
		{
			btnMusic.Icon = music;
		}
		else
		{
			btnMusic.Icon = noMusic;
		}
	}

}
