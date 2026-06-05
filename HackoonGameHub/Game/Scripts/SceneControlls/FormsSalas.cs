using Godot;
using System;
using System.Collections.Generic;
using Game.Scripts;
using Models;
using Repository.Exceptions;
using Repository.Postgress;
using Service;

public partial class FormsSalas : Control
{
	private VBoxContainer containerSalas;
	private Label helloLabel;
	
	private Button saveButton;
	private Button cancelarButton;
	
	private LineEdit nomeEdit;
	private LineEdit descricaoEdit;

	private ReturnScene _window;

	private bool _editing = false;

	private Sala _salaToEdit; 
	
	public override async void _Ready()
	{
		helloLabel = GetNode<Label>("helloLabel");
		var session = GetNode<SessionManager>("/root/SessionManager");
		Professor professor = (Professor) session.usuario;
		
		helloLabel.Text += professor.name;
		
		containerSalas = GetNode<VBoxContainer>("HBoxContainer/PanelSalas/containerSalas");
		
		saveButton = GetNode<Button>("HBoxContainer/PanelFormsSalas/VBoxContainer/HBoxContainer/salaButton");
		cancelarButton = GetNode<Button>("HBoxContainer/PanelFormsSalas/VBoxContainer/HBoxContainer/cancelarButton");
		
		nomeEdit = GetNode<LineEdit>("HBoxContainer/PanelFormsSalas/VBoxContainer/nomeEdit");
		descricaoEdit = GetNode<LineEdit>("HBoxContainer/PanelFormsSalas/VBoxContainer/descricaoEdit");
		
		cancelarButton.Visible = false;
		cancelarButton.Pressed += OnCancelarPressed;
		saveButton.Pressed += OnSavePressed;
		
		List<Sala> salas = await SalaService.ReadAll();
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
				popup.AddItem("Editar", 1);
				popup.AddItem("Excluir", 2);
				
				containerSalas.AddChild(newButton);

				popup.IdPressed += (idItemClicado) => OnPoputPressed(idItemClicado, sala);
			}
		}
		
		_window = GetNode<ReturnScene>("Control");
		
	}

	public async void OnSavePressed()
	{
		if (_editing)
		{
			if (!string.Equals(nomeEdit.Text, _salaToEdit.name))
			{
				_salaToEdit.name = nomeEdit.Text;
			}

			if (!string.Equals(descricaoEdit.Text, _salaToEdit.descricao))
			{
				_salaToEdit.descricao = descricaoEdit.Text;
			}

			try
			{
				await SalaService.Update(_salaToEdit, _salaToEdit.id);
				
				nomeEdit.Text = "";
				descricaoEdit.Text = "";
				cancelarButton.Visible = false;
				_editing = false;
				_salaToEdit = new Sala();
				
				GetTree().ReloadCurrentScene();
				
			}
			catch (ResourceNotFoundException e)
			{
				_window.subtitleLabel.Text = "ERRO";
				_window.descriptionLabel.Text = e.Message;
				_window.Show();
				nomeEdit.Text = "";
				descricaoEdit.Text = "";
				cancelarButton.Visible = false;
				_editing = false;
				_salaToEdit = new Sala();
			}
			
		}
		else
		{
			try
			{
				Sala sala = new Sala(nomeEdit.Text, descricaoEdit.Text);

				await SalaService.CreateSala(sala);

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
				popup.AddItem("Editar", 1);
				popup.AddItem("Excluir", 2);

				containerSalas.AddChild(newButton);

				popup.IdPressed += (idItemClicado) => OnPoputPressed(idItemClicado, sala);
			}
			catch (InvalidParameterException e)
			{
				_window.subtitleLabel.Text = "ERRO";
				_window.descriptionLabel.Text = e.Message;
				_window.Show();
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}
		}
	}

	public void OnCancelarPressed()
	{
		nomeEdit.Text = "";
		descricaoEdit.Text = "";
		cancelarButton.Visible = false;
		_editing = false;
		_salaToEdit = new Sala();
	}

	public async void OnPoputPressed(long id, Sala sala)
	{
		if (id == 0)
		{
			
		}
		else if (id == 1)
		{
			_editing = true;
			cancelarButton.Visible = true;
			nomeEdit.Text = sala.name;
			descricaoEdit.Text = sala.descricao;
			_salaToEdit = sala;
			
		}else if (id == 2)
		{
			await SalaService.Delete(sala.id);
			GetTree().ReloadCurrentScene();
		}
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override async void _Process(double delta)
	{
		
	}
}
