using Godot;
using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;
using Game.Scripts;
using Models;
using Service;

public partial class LobbyPanel : Control
{
	private Label _nomeSalaLabel;

	private VBoxContainer _usernameContainer;
	private VBoxContainer _nameContainer;
	private VBoxContainer _middleContainer;
	
	private SessionManager session;
	
	public override async void _Ready()
	{
		_nomeSalaLabel = GetNode<Label>("PanelContainer/VBoxContainer/HBoxContainer/nomeSalaLabel");
		_usernameContainer = GetNode<VBoxContainer>("PanelContainer/VBoxContainer/alunosHBoxContainer/usernameVBoxContainer");
		_nameContainer = GetNode<VBoxContainer>("PanelContainer/VBoxContainer/alunosHBoxContainer/nomeVBoxContainer");
		_middleContainer = GetNode<VBoxContainer>("PanelContainer/VBoxContainer/alunosHBoxContainer/middleVBoxContainer");
		
		session = GetNode<SessionManager>("/root/SessionManager");

		_nomeSalaLabel.Text = session.sala.name;

		List<Aluno> alunos = await AlunoService.showAll();

		
		
		foreach (Aluno aluno in alunos)
		{
			Label name = new Label();
			Label username = new Label();
			
			Label middle = new Label();
			middle.Text = "|";
			
			name.Text = aluno.name;
			_nameContainer.AddChild(name);
			
			_middleContainer.AddChild(middle);
			
			username.Text = aluno.username;
			_usernameContainer.AddChild(username);
		}
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override async void _Process(double delta)
	{
		await ProfessorService.SendMessage(session.usuario.name);
	}
}
