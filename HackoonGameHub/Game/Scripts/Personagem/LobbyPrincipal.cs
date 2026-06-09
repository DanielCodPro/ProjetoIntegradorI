using Godot;
using System;

public partial class LobbyPrincipal : Node2D
{
	private ENetMultiplayerPeer _peer = new();
	
	[Export] public PackedScene playerScene { get; set; }
	[Export] public MultiplayerSpawner multiplayerSpawner { get; set; }

	public override void _Ready()
	{
		GetNode<Button>("UI/hostButton").Pressed += OnHostPressed;
		GetNode<Button>("UI/entrarButton").Pressed += OnEntrarPressed;

		if (multiplayerSpawner != null)
		{
			// CONFIGURAÇÃO CRÍTICA: Dizemos ao spawner como instanciar o jogador corretamente
			multiplayerSpawner.SpawnFunction = Callable.From<Variant, Node>(CustomSpawnFunction);
		}
	}

	private void OnHostPressed()
	{
		var error = _peer.CreateServer(7777);
		if (error != Error.Ok)
		{
			GD.PrintErr("Erro ao criar servidor: " + error);
			return;
		}
	   
		Multiplayer.PeerConnected += AddPlayer;
		Multiplayer.PeerDisconnected += RemovePlayer;
	   
		Multiplayer.MultiplayerPeer = _peer;
		GD.Print("Servidor Iniciado!");
	   
		// Cria o player do Host
		AddPlayer(1);
	}

	private void OnEntrarPressed()
	{
		var error = _peer.CreateClient("127.0.0.1", 7777);
		if (error != Error.Ok)
		{
			GD.PrintErr("Erro ao conectar ao servidor: " + error);
			return;
		}
	   
		Multiplayer.MultiplayerPeer = _peer;
		GD.Print("Tentando conectar como cliente...");
	}

	private void AddPlayer(long id)
	{
		if (multiplayerSpawner == null)
		{
			GD.PrintErr("ERRO: multiplayerSpawner não foi atribuído no Inspetor!");
			return;
		}

		// Em vez de dar Instantiate e AddChild manualmente, chamamos o Spawner do próprio Godot.
		// O argumento 'id' será enviado para a nossa CustomSpawnFunction automaticamente.
		multiplayerSpawner.Spawn(id);
		GD.Print($"Jogador {id} enviado para o Spawner.");
	}

	// Essa função roda NO SERVIDOR E NO CLIENTE no exato momento em que o objeto é criado na rede!
	private Node CustomSpawnFunction(Variant data)
	{
		long id = (long)data;

		if (playerScene == null)
		{
			GD.PrintErr("ERRO: playerScene não foi atribuída no Inspetor!");
			return null;
		}

		var player = playerScene.Instantiate<Personagem>();
		player.Name = id.ToString();
		
		// Configura a autoridade AQUI (antes do nó entrar na árvore de cena)
		player.SetMultiplayerAuthority((int)id);

		var synchronizer = player.GetNodeOrNull<MultiplayerSynchronizer>("MultiplayerSynchronizer");
		if (synchronizer != null)
		{
			synchronizer.SetMultiplayerAuthority((int)id);
		}

		// Retornamos o nó. O próprio MultiplayerSpawner vai fazer o AddChild de forma segura e síncrona.
		return player;
	}

	private void RemovePlayer(long id)
	{
		var player = GetNodeOrNull(id.ToString());
		if (player != null)
		{
			player.QueueFree();
		}
	}

	public override void _ExitTree()
	{
		Multiplayer.PeerConnected -= AddPlayer;
		Multiplayer.PeerDisconnected -= RemovePlayer;
	}
}
