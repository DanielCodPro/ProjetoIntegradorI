using Godot;
using System;
using System.Collections.Generic;
using Game.Scripts;
using Models;
using Repository.Postgress;

public partial class FormsSalas : Control
{
	private VBoxContainer containerSalas;
	private Label helloLabel;
	
	private Button saveButton;
	private LineEdit nomeEdit;
	private LineEdit descricaoEdit;
	
	public override async void _Ready()
	{
		helloLabel = GetNode<Label>("helloLabel");
		var session = GetNode<SessionManager>("/root/SessionManager");
		Professor professor = (Professor) session.usuario;
		helloLabel.Text += professor.name;
		
		containerSalas = GetNode<VBoxContainer>("HBoxContainer/PanelSalas/containerSalas");
		
		saveButton = GetNode<Button>("HBoxContainer/PanelFormsSalas/VBoxContainer/salaButton");
		nomeEdit = GetNode<LineEdit>("HBoxContainer/PanelFormsSalas/VBoxContainer/nomeEdit");
		descricaoEdit = GetNode<LineEdit>("HBoxContainer/PanelFormsSalas/VBoxContainer/descricaoEdit");

		saveButton.Pressed += OnSavePressed;
		
		List<Sala> salas = await SalaDB.ReadAll();
		foreach (Sala sala in salas)
		{
			if (sala != null)
			{
				MenuButton newButton = new MenuButton();
				
				if (!string.IsNullOrEmpty(sala.descricao))
				{
					newButton.Text = sala.name + " : " + sala.descricao;
				}
				else
				{
					newButton.Text = sala.name;
				}
				
				PopupMenu popup =  newButton.GetPopup();
				popup.AddItem("Entrar na Sala", 0);
				popup.AddItem("Excluir", 1);
				
				containerSalas.AddChild(newButton);

				popup.IdPressed += (idItemClicado) => OnPoputPressed(idItemClicado, sala);
			}
		}
	}

	public async void OnSavePressed()
	{
		try
		{
			Sala sala = new Sala(nomeEdit.Text, descricaoEdit.Text);
			
			await SalaDB.Create(sala);
			
			MenuButton newButton = new MenuButton();

			if (!string.IsNullOrEmpty(sala.descricao))
			{
				newButton.Text = sala.name + " : " + sala.descricao;
			}
			else
			{
				newButton.Text = sala.name;
			}
			
			
			PopupMenu popup = newButton.GetPopup();
			popup.AddItem("Entrar na Sala", 0);
			popup.AddItem("Excluir", 1);
			
			containerSalas.AddChild(newButton);
			
			popup.IdPressed += (idItemClicado) => OnPoputPressed(idItemClicado, sala);
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			throw;
		}
	}

	public async void OnPoputPressed(long id, Sala sala)
	{
		if (id == 0)
		{
			
		}else if (id == 1)
		{
			await SalaDB.Delete(sala.id);
			GetTree().ReloadCurrentScene();
		}
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override async void _Process(double delta)
	{
		
	}
}
