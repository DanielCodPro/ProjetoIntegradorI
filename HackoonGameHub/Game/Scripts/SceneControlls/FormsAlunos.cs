using Godot;
using System;
using System.Collections.Generic;
using Models;
using Service;

public partial class FormsAlunos : Control
{
	private VBoxContainer _nomeContainer;
	private VBoxContainer _sobrenomeContainer;
	private VBoxContainer _buttonContainer;
	
	public override async void _Ready()
	{
		_nomeContainer = GetNode<VBoxContainer>("PanelContainer/alunosHBoxContainer/nomeVBoxContainer");
		_sobrenomeContainer = GetNode<VBoxContainer>("PanelContainer/alunosHBoxContainer/sobrenomeVBoxContainer");
		_buttonContainer = GetNode<VBoxContainer>("PanelContainer/alunosHBoxContainer/buttonContainer");
		
		List<Aluno> alunos = await AlunoService.showAll();

		foreach (Aluno aluno in alunos)
		{
			Label nome = new Label();
			nome.Text = aluno.name;

			Label username = new Label();
			username.Text = "| " + aluno.username;
			
			Button button = new Button();
			button.Text = "Entrar";
			
			_nomeContainer.AddChild(nome);
			_sobrenomeContainer.AddChild(username);
			_buttonContainer.AddChild(button);
			
			int idAluno = aluno.id;
			button.Pressed += () =>
			{
				GD.Print($"[EVENTO] Clicou para entrar! Enviando ID: {idAluno} ({nome.Text})");
				AlunoService.SendMyPersona(idAluno);
			};

		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
