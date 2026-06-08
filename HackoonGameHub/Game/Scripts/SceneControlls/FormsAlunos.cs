using Godot;
using System;
using System.Collections.Generic;
using Models;
using Service;

public partial class FormsAlunos : Control
{
	private VBoxContainer _nomeContainer;
	private VBoxContainer _sobrenomeContainer;
	
	public override async void _Ready()
	{
		_nomeContainer = GetNode<VBoxContainer>("PanelContainer/alunosHBoxContainer/nomeVBoxContainer");
		_sobrenomeContainer = GetNode<VBoxContainer>("PanelContainer/alunosHBoxContainer/sobrenomeVBoxContainer");

		List<Aluno> alunos = await AlunoService.showAll();

		foreach (Aluno aluno in alunos)
		{
			MenuButton nomeButton = new MenuButton();
			nomeButton.Text = aluno.name;

			PopupMenu popup = nomeButton.GetPopup();
			popup.AddItem("Entrar", 0);

			Label username = new Label();
			username.Text = aluno.username;
			
			_nomeContainer.AddChild(nomeButton);
			_sobrenomeContainer.AddChild(username);
			
			
			
		}
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
