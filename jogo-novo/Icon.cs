using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class Icon : Control
{
	private readonly string caminhoVerso = "res://ImagensLetras/alfabeto.jpg";
	private readonly string caminhoFundo = "res://ImagensLetras/fundo.jpg"; 

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
	private Panel vitoriaPanel;
	private PanelContainer gameOverCard;
	private PanelContainer vitoriaCard;
	private static readonly Vector2 TamanhoCardFim = new Vector2(420, 300);

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
		CriarTelaVitoria();
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

	// Cria um StyleBoxFlat simples com cantos arredondados
	private StyleBoxFlat CriarEstiloCaixa(Color cor, int raio, int bordaLargura = 0, Color? corBorda = null)
	{
		StyleBoxFlat style = new StyleBoxFlat();
		style.BgColor = cor;
		style.SetCornerRadiusAll(raio);
		style.SetContentMarginAll(30);
		if (bordaLargura > 0)
		{
			style.SetBorderWidthAll(bordaLargura);
			style.BorderColor = corBorda ?? Colors.White;
		}
		return style;
	}

	// Aplica um visual de botão "pílula" com hover/pressed nas cores informadas
	private void EstilizarBotao(Button btn, Color corBase)
	{
		StyleBoxFlat normal = CriarEstiloCaixa(corBase, 16);
		StyleBoxFlat hover = CriarEstiloCaixa(corBase.Lightened(0.15f), 16);
		StyleBoxFlat pressed = CriarEstiloCaixa(corBase.Darkened(0.15f), 16);

		normal.ContentMarginTop = 12;
		normal.ContentMarginBottom = 12;
		normal.ContentMarginLeft = 26;
		normal.ContentMarginRight = 26;
		hover.ContentMarginTop = normal.ContentMarginTop;
		hover.ContentMarginBottom = normal.ContentMarginBottom;
		hover.ContentMarginLeft = normal.ContentMarginLeft;
		hover.ContentMarginRight = normal.ContentMarginRight;
		pressed.ContentMarginTop = normal.ContentMarginTop;
		pressed.ContentMarginBottom = normal.ContentMarginBottom;
		pressed.ContentMarginLeft = normal.ContentMarginLeft;
		pressed.ContentMarginRight = normal.ContentMarginRight;

		btn.AddThemeStyleboxOverride("normal", normal);
		btn.AddThemeStyleboxOverride("hover", hover);
		btn.AddThemeStyleboxOverride("pressed", pressed);
		btn.AddThemeStyleboxOverride("focus", hover);
		btn.AddThemeColorOverride("font_color", Colors.White);
		btn.AddThemeColorOverride("font_hover_color", Colors.White);
		btn.AddThemeColorOverride("font_pressed_color", Colors.White);
		btn.AddThemeFontSizeOverride("font_size", 18);
	}

	private void CriarTelaGameOver()
	{
		gameOverPanel = new Panel();
		gameOverPanel.SetAnchorsPreset(LayoutPreset.FullRect);
		gameOverPanel.Visible = false;
		gameOverPanel.MouseFilter = Control.MouseFilterEnum.Stop;
		// Fundo escurecido semi-transparente cobrindo a tela toda
		gameOverPanel.AddThemeStyleboxOverride("panel", CriarEstiloCaixa(new Color(0, 0, 0, 0.75f), 0));

		gameOverCard = new PanelContainer();
		gameOverCard.SetAnchorsPreset(LayoutPreset.Center);
		gameOverCard.CustomMinimumSize = TamanhoCardFim;
		gameOverCard.AddThemeStyleboxOverride(
			"panel",
			CriarEstiloCaixa(new Color(0.12f, 0.07f, 0.09f, 0.98f), 24, 3, new Color(0.85f, 0.18f, 0.22f))
		);

		VBoxContainer vbox = new VBoxContainer();
		vbox.Alignment = BoxContainer.AlignmentMode.Center;
		vbox.AddThemeConstantOverride("separation", 18);
		vbox.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
		vbox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;

		Label lblEmoji = new Label();
		lblEmoji.Text = "💔";
		lblEmoji.HorizontalAlignment = HorizontalAlignment.Center;
		lblEmoji.AddThemeFontSizeOverride("font_size", 44);

		Label lblGameOver = new Label();
		lblGameOver.Text = "GAME OVER";
		lblGameOver.HorizontalAlignment = HorizontalAlignment.Center;
		lblGameOver.AddThemeFontSizeOverride("font_size", 34);
		lblGameOver.AddThemeColorOverride("font_color", new Color(0.95f, 0.3f, 0.32f));

		Label lblSub = new Label();
		lblSub.Text = "Suas vidas acabaram. Que tal tentar de novo?";
		lblSub.HorizontalAlignment = HorizontalAlignment.Center;
		lblSub.AutowrapMode = TextServer.AutowrapMode.WordSmart;
		lblSub.AddThemeFontSizeOverride("font_size", 15);
		lblSub.AddThemeColorOverride("font_color", new Color(0.82f, 0.78f, 0.78f));

		Button btnReiniciar = new Button();
		btnReiniciar.Text = "🔄  Tentar Novamente";
		EstilizarBotao(btnReiniciar, new Color(0.82f, 0.2f, 0.26f));
		btnReiniciar.Pressed += () =>
		{
			gameOverPanel.Visible = false;
			bloqueado = false;
			faseAtualIndex = 0;
			CarregarFase(faseAtualIndex);
		};

		vbox.AddChild(lblEmoji);
		vbox.AddChild(lblGameOver);
		vbox.AddChild(lblSub);
		vbox.AddChild(btnReiniciar);
		gameOverCard.AddChild(vbox);
		gameOverPanel.AddChild(gameOverCard);
		AddChild(gameOverPanel);
	}

	// Tela de vitória exibida ao concluir todas as fases
	private void CriarTelaVitoria()
	{
		vitoriaPanel = new Panel();
		vitoriaPanel.SetAnchorsPreset(LayoutPreset.FullRect);
		vitoriaPanel.Visible = false;
		vitoriaPanel.MouseFilter = Control.MouseFilterEnum.Stop;
		vitoriaPanel.AddThemeStyleboxOverride("panel", CriarEstiloCaixa(new Color(0, 0, 0, 0.75f), 0));

		vitoriaCard = new PanelContainer();
		vitoriaCard.SetAnchorsPreset(LayoutPreset.Center);
		vitoriaCard.CustomMinimumSize = TamanhoCardFim;
		vitoriaCard.AddThemeStyleboxOverride(
			"panel",
			CriarEstiloCaixa(new Color(0.09f, 0.1f, 0.07f, 0.98f), 24, 3, new Color(1.0f, 0.82f, 0.2f))
		);

		VBoxContainer vbox = new VBoxContainer();
		vbox.Alignment = BoxContainer.AlignmentMode.Center;
		vbox.AddThemeConstantOverride("separation", 18);
		vbox.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
		vbox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;

		Label lblEmoji = new Label();
		lblEmoji.Text = "🎉";
		lblEmoji.HorizontalAlignment = HorizontalAlignment.Center;
		lblEmoji.AddThemeFontSizeOverride("font_size", 44);

		Label lblVitoria = new Label();
		lblVitoria.Text = "VITÓRIA!";
		lblVitoria.HorizontalAlignment = HorizontalAlignment.Center;
		lblVitoria.AddThemeFontSizeOverride("font_size", 34);
		lblVitoria.AddThemeColorOverride("font_color", new Color(1.0f, 0.85f, 0.3f));

		Label lblSub = new Label();
		lblSub.Text = "Parabéns! Você completou todas as fases!";
		lblSub.HorizontalAlignment = HorizontalAlignment.Center;
		lblSub.AutowrapMode = TextServer.AutowrapMode.WordSmart;
		lblSub.AddThemeFontSizeOverride("font_size", 15);
		lblSub.AddThemeColorOverride("font_color", new Color(0.85f, 0.88f, 0.8f));

		Button btnJogarNovamente = new Button();
		btnJogarNovamente.Text = "🔁  Jogar Novamente";
		EstilizarBotao(btnJogarNovamente, new Color(0.25f, 0.6f, 0.32f));
		btnJogarNovamente.Pressed += () =>
		{
			vitoriaPanel.Visible = false;
			bloqueado = false;
			faseAtualIndex = 0;
			CarregarFase(faseAtualIndex);
		};

		vbox.AddChild(lblEmoji);
		vbox.AddChild(lblVitoria);
		vbox.AddChild(lblSub);
		vbox.AddChild(btnJogarNovamente);
		vitoriaCard.AddChild(vbox);
		vitoriaPanel.AddChild(vitoriaCard);
		AddChild(vitoriaPanel);
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
			VidasIniciais = 6,
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
			VidasIniciais = 10,
			TamanhoCarta = new Vector2(150, 120),
			ImagensFrente = new string[]
			{
				"res://ImagensLetras/maca.jpg",
				"res://ImagensLetras/banana.jpg",
				"res://ImagensLetras/laranja.jpg",
				"res://ImagensLetras/uva.jpg",
				"res://ImagensLetras/Abacaxi.png",
				"res://ImagensLetras/goiaba.jpg",
				"res://ImagensLetras/melancia.jpeg",
				"res://ImagensLetras/pera.png"
			},
			ImagensTexto = new string[]
			{
				"res://ImagensLetras/MaçaP.jpg",
				"res://ImagensLetras/bananaP.png",
				"res://ImagensLetras/LaranjaP.png",
				"res://ImagensLetras/UvaP.jpg",
				"res://ImagensLetras/AbacaxiP.png",
				"res://ImagensLetras/GoiabaP.png",
				"res://ImagensLetras/MelanciaP.png",
				"res://ImagensLetras/PeraP.png"
			}
		});
	}

	private void CarregarFase(int indexFase)
	{
		if (indexFase < 0 || indexFase >= fases.Count)
		{
			GD.Print("Você completou todas as fases!");
			ExibirVitoria();
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

		// Reseta seleção ao gerar uma nova fase
		cartaSelecionada1 = null;
		cartaSelecionada2 = null;

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
		// Ignora clique se: o jogo está bloqueado, a carta já está desabilitada
		// (já foi acertada), ou é a mesma carta já selecionada.
		if (bloqueado || carta.Disabled || carta == cartaSelecionada1 || carta == cartaSelecionada2)
			return;

		// --- CORREÇÃO DE BUG: reserva o "slot" da carta ANTES do await ---
		// Antes, a carta só era registrada como selecionada depois da animação
		// terminar. Isso permitia que um clique numa 3ª carta, feito durante a
		// animação da 2ª, sobrescrevesse "cartaSelecionada2" e deixasse a carta
		// anterior virada na tela sem nunca ser comparada ou desvirada.
		bool éSegundaCarta = cartaSelecionada1 != null;

		if (!éSegundaCarta)
		{
			cartaSelecionada1 = carta;
		}
		else
		{
			cartaSelecionada2 = carta;
			bloqueado = true; // bloqueia novos cliques imediatamente
		}

		Texture2D imagemFrente = (Texture2D)carta.GetMeta("imagem_frente");
		await AnimarGiro(carta, imagemFrente);

		if (éSegundaCarta)
		{
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

	// Anima a entrada de uma tela final: fundo em fade-in e cartão "estourando" (pop) na tela
	private void AnimarEntradaTelaFinal(Panel painel, PanelContainer card)
	{
		painel.Modulate = new Color(1, 1, 1, 0);
		painel.Visible = true;

		card.PivotOffset = card.CustomMinimumSize / 2;
		card.Scale = new Vector2(0.7f, 0.7f);

		Tween tween = CreateTween().SetParallel(true);
		tween.TweenProperty(painel, "modulate:a", 1.0f, 0.25f);
		tween.TweenProperty(card, "scale", Vector2.One, 0.35f)
			.SetTrans(Tween.TransitionType.Back)
			.SetEase(Tween.EaseType.Out);
	}

	private void ExibirGameOver()
	{
		bloqueado = true;
		AnimarEntradaTelaFinal(gameOverPanel, gameOverCard);
	}

	private void ExibirVitoria()
	{
		bloqueado = true;
		AnimarEntradaTelaFinal(vitoriaPanel, vitoriaCard);
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