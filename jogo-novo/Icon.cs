using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class Icon : Control
{
	private readonly string caminhoVerso = "res://ImagensLetras/alfabeto.jpg";
	private readonly string caminhoFundo = "res://ImagensLetras/Fundo.jpg"; 

	private class DadosCarta
	{
		public int Id { get; set; }
		public Texture2D ImagemFrente { get; set; }
	}

	public class DadosFase
	{
		public int NumeroColunas { get; set; } = 4;
		public int VidasIniciais { get; set; } = 3;
		public Vector2 TamanhoCarta { get; set; } = new Vector2(200, 150);
		public string[] ImagensFrente { get; set; }
		public string[] ImagensTexto { get; set; } 
	}

	private List<DadosFase> fases = new List<DadosFase>();
	private int faseAtualIndex = 0;
	private int vidasAtuais;

	private TextureButton cartaSelecionada1 = null;
	private TextureButton cartaSelecionada2 = null;
	private bool bloqueado = false;
	private GridContainer grid;
	private HBoxContainer containerVidas;
	private Panel gameOverPanel;

	public override void _Ready()
	{
		grid = GetNodeOrNull<GridContainer>("GridContainer");
		containerVidas = GetNodeOrNull<HBoxContainer>("ContainerVidas");

		if (grid == null || containerVidas == null)
		{
			GD.PrintErr("Erro: 'GridContainer' ou 'ContainerVidas' não foi encontrado na cena!");
			return;
		}

		// --- ADICIONA O FUNDO AUTOMATICAMENTE ---
		CriarFundo();
		// ----------------------------------------

		// Centralização do Grid na tela
		SetAnchorsPreset(LayoutPreset.FullRect);
		grid.SetAnchorsPreset(LayoutPreset.Center);
		grid.GrowHorizontal = GrowDirection.Both;
		grid.GrowVertical = GrowDirection.Both;

		CriarTelaGameOver();
		InicializarFases();
		CarregarFase(faseAtualIndex);
	}

	private void CriarFundo()
	{
		Texture2D texFundo = GD.Load<Texture2D>(caminhoFundo);

		if (texFundo != null)
		{
			TextureRect fundo = new TextureRect();
			fundo.Texture = texFundo;
			fundo.SetAnchorsPreset(LayoutPreset.FullRect);
			fundo.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
			fundo.StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered;

			AddChild(fundo);
			MoveChild(fundo, 0); // Envia para o fundo (atrás das cartas)
		}
		else
		{
			GD.PrintErr($"Aviso: Imagem de fundo não encontrada em '{caminhoFundo}'. Usando cor padrão.");
			
			ColorRect fundoCor = new ColorRect();
			fundoCor.Color = new Color(0.12f, 0.15f, 0.22f);
			fundoCor.SetAnchorsPreset(LayoutPreset.FullRect);

			AddChild(fundoCor);
			MoveChild(fundoCor, 0);
		}
	}

	private void CriarTelaGameOver()
	{
		gameOverPanel = new Panel();
		gameOverPanel.SetAnchorsPreset(LayoutPreset.FullRect);
		gameOverPanel.Visible = false;

		VBoxContainer vbox = new VBoxContainer();
		vbox.SetAnchorsPreset(LayoutPreset.Center);

		Label lblGameOver = new Label();
		lblGameOver.Text = "GAME OVER";
		lblGameOver.HorizontalAlignment = HorizontalAlignment.Center;

		Button btnReiniciar = new Button();
		btnReiniciar.Text = "Tentar Novamente";
		btnReiniciar.Pressed += () =>
		{
			gameOverPanel.Visible = false;
			bloqueado = false;
			CarregarFase(faseAtualIndex);
		};

		vbox.AddChild(lblGameOver);
		vbox.AddChild(btnReiniciar);
		gameOverPanel.AddChild(vbox);
		AddChild(gameOverPanel);
	}

	private void InicializarFases()
	{
		// FASE 1
		fases.Add(new DadosFase
		{
			NumeroColunas = 2,
			VidasIniciais = 4,
			TamanhoCarta = new Vector2(200, 150),
			ImagensFrente = new string[]
			{
				"res://ImagensLetras/a.jpg",
                "res://ImagensLetras/A.png"
			}
		});

		// FASE 2
		fases.Add(new DadosFase
		{
			NumeroColunas = 4,
			VidasIniciais = 7,
			TamanhoCarta = new Vector2(180, 135),
			ImagensFrente = new string[]
			{
				"res://ImagensLetras/a.jpg",
				"res://ImagensLetras/A.png",
				"res://ImagensLetras/b.jpg",
                "res://ImagensLetras/bm.png"
			}
		});

		// FASE 3 (Caminhos de extensão corrigidos)
		fases.Add(new DadosFase
		{
			NumeroColunas = 4,
			VidasIniciais = 4,
			TamanhoCarta = new Vector2(150, 120),
			ImagensFrente = new string[]
			{
				"res://ImagensLetras/maca.jpg",
				"res://ImagensLetras/banana.jpg",
				"res://ImagensLetras/laranja.jpg",
				"res://ImagensLetras/uva.jpg",
				"res://ImagensLetras/AbacaxiP.png",
				"res://ImagensLetras/goiaba.jpg",
                "res://ImagensLetras/melancia.jpeg"
			},
			ImagensTexto = new string[]
			{
				"res://ImagensLetras/MacaP.jpg",
				"res://ImagensLetras/BananaP.png",
				"res://ImagensLetras/LaranjaP.png",
				"res://ImagensLetras/UvaP.jpg",
				"res://ImagensLetras/AbacaxiP.png",
				"res://ImagensLetras/GoiabaP.png",
                "res://ImagensLetras/MelanciaP.png"
			}
		});
	}

	private void CarregarFase(int indexFase)
	{
		if (indexFase < 0 || indexFase >= fases.Count)
		{
			GD.Print("Você completou todas as fases!");
			return;
		}

		foreach (Node child in grid.GetChildren())
		{
			child.QueueFree();
		}

		DadosFase fase = fases[indexFase];
		vidasAtuais = fase.VidasIniciais;

		AtualizarBarraVidas();

		grid.Columns = fase.NumeroColunas;
		GerarEEmbaralharCartas(grid, fase);
	}

	private void AtualizarBarraVidas()
	{
		var coracoes = containerVidas.GetChildren();
		for (int i = 0; i < coracoes.Count; i++)
		{
			if (coracoes[i] is CanvasItem coracao)
			{
				coracao.Visible = i < vidasAtuais;
			}
		}
	}

	private void GerarEEmbaralharCartas(GridContainer grid, DadosFase fase)
	{
		Texture2D imagemVerso = GD.Load<Texture2D>(caminhoVerso);
		
		if (imagemVerso == null)
		{
			GD.PrintErr($"Erro: Imagem do verso não encontrada em {caminhoVerso}");
			return;
		}

		List<DadosCarta> listaCartas = new List<DadosCarta>();

		for (int i = 0; i < fase.ImagensFrente.Length; i++)
		{
			Texture2D tex1 = GD.Load<Texture2D>(fase.ImagensFrente[i]);
			
			if (tex1 == null)
			{
				GD.PrintErr($"Erro ao carregar imagem: {fase.ImagensFrente[i]}");
				continue;
			}

			Texture2D tex2 = tex1;
			if (fase.ImagensTexto != null && i < fase.ImagensTexto.Length)
			{
				tex2 = GD.Load<Texture2D>(fase.ImagensTexto[i]);
			}

			listaCartas.Add(new DadosCarta { Id = i, ImagemFrente = tex1 });
			listaCartas.Add(new DadosCarta { Id = i, ImagemFrente = tex2 });
		}

		Random rng = new Random();
		int n = listaCartas.Count;
		while (n > 1)
		{
			n--;
			int k = rng.Next(n + 1);
			DadosCarta temp = listaCartas[k];
			listaCartas[k] = listaCartas[n];
			listaCartas[n] = temp;
		}

		foreach (DadosCarta dados in listaCartas)
		{
			TextureButton btn = new TextureButton();
			btn.TextureNormal = imagemVerso;
			btn.IgnoreTextureSize = true;
			btn.StretchMode = TextureButton.StretchModeEnum.Scale;
			btn.CustomMinimumSize = fase.TamanhoCarta;

			btn.MouseFilter = Control.MouseFilterEnum.Stop;

			btn.SetMeta("id", dados.Id);
			btn.SetMeta("imagem_frente", dados.ImagemFrente);

			btn.Pressed += () => OnCartaPressed(btn);

			grid.AddChild(btn);
		}
	}

	private async void OnCartaPressed(TextureButton carta)
	{
		if (bloqueado || carta == cartaSelecionada1)
			return;

		Texture2D imagemFrente = (Texture2D)carta.GetMeta("imagem_frente");

		await AnimarGiro(carta, imagemFrente);

		if (cartaSelecionada1 == null)
		{
			cartaSelecionada1 = carta;
		}
		else
		{
			cartaSelecionada2 = carta;
			bloqueado = true;
			await VerificarPar();
			bloqueado = false;
		}
	}

	private async Task VerificarPar()
	{
		int id1 = (int)cartaSelecionada1.GetMeta("id");
		int id2 = (int)cartaSelecionada2.GetMeta("id");

		if (id1 == id2)
		{
			cartaSelecionada1.Disabled = true;
			cartaSelecionada2.Disabled = true;

			Tween tweenSucesso = CreateTween().SetParallel(true);
			tweenSucesso.TweenProperty(cartaSelecionada1, "scale", new Vector2(1.1f, 1.1f), 0.1f);
			tweenSucesso.TweenProperty(cartaSelecionada2, "scale", new Vector2(1.1f, 1.1f), 0.1f);
			await ToSignal(tweenSucesso, Tween.SignalName.Finished);

			Tween tweenRetorno = CreateTween().SetParallel(true);
			tweenRetorno.TweenProperty(cartaSelecionada1, "scale", Vector2.One, 0.1f);
			tweenRetorno.TweenProperty(cartaSelecionada2, "scale", Vector2.One, 0.1f);

			cartaSelecionada1 = null;
			cartaSelecionada2 = null;

			VerificarFimDaFase();
		}
		else
		{
			vidasAtuais--;
			AtualizarBarraVidas();

			await ToSignal(GetTree().CreateTimer(1.0), SceneTreeTimer.SignalName.Timeout);

			Texture2D verso = GD.Load<Texture2D>(caminhoVerso);

			Task t1 = AnimarGiro(cartaSelecionada1, verso);
			Task t2 = AnimarGiro(cartaSelecionada2, verso);
			await Task.WhenAll(t1, t2);

			cartaSelecionada1 = null;
			cartaSelecionada2 = null;

			if (vidasAtuais <= 0)
			{
				ExibirGameOver();
			}
		}
	}

	private void ExibirGameOver()
	{
		bloqueado = true;
		gameOverPanel.Visible = true;
	}

	private void VerificarFimDaFase()
	{
		bool todasDesabilitadas = true;
		foreach (Node node in grid.GetChildren())
		{
			if (node is TextureButton btn && !btn.Disabled)
			{
				todasDesabilitadas = false;
				break;
			}
		}

		if (todasDesabilitadas)
		{
			GD.Print($"Fase {faseAtualIndex + 1} Concluída!");
			ProximaFase();
		}
	}

	private async void ProximaFase()
	{
		await Task.Delay(1000);
		faseAtualIndex++;
		CarregarFase(faseAtualIndex);
	}

	private async Task AnimarGiro(TextureButton carta, Texture2D novaTextura)
	{
		carta.PivotOffset = carta.CustomMinimumSize / 2;

		Tween tween1 = CreateTween().SetParallel(true);
		tween1.TweenProperty(carta, "scale:x", 0.05f, 0.15f);
		tween1.TweenProperty(carta, "scale:y", 1.05f, 0.15f);
		await ToSignal(tween1, Tween.SignalName.Finished);

		carta.TextureNormal = novaTextura;

		Tween tween2 = CreateTween().SetParallel(true);
		tween2.TweenProperty(carta, "scale:x", 1.0f, 0.15f);
		tween2.TweenProperty(carta, "scale:y", 1.0f, 0.15f);
		await ToSignal(tween2, Tween.SignalName.Finished);
	}
}
