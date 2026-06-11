using System.Net;
using System.Runtime.InteropServices.ComTypes;
using System.Text.Json;
using Models;
using Repository.Exceptions;
using Repository.Postgress;

namespace Service;

public class AlunoService
{
    public static async void RegisterAluno(Aluno aluno)
    {
        try
        {
            if (!string.IsNullOrEmpty(aluno.password))
            {
                aluno.password = SecurityService.GerarHaskSenha(aluno.password);
            }
            await AlunoDB.Create(aluno);
        }
        catch (InvalidParameterException ex)
        {
            Console.WriteLine(ex.Message);
        }
        catch (ResourceAlreadyExistsException ex)
        {
            Console.WriteLine(ex.Message);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        
    }
    public static async Task<Aluno?> LoginAluno(string username, string? password)
    {
        Aluno aluno;
        try
        {
            aluno = await AlunoDB.GetAlunoByUsername(username);
            if (!string.IsNullOrEmpty(aluno.password))
            {
                if (string.IsNullOrEmpty(password))
                {
                    throw new ResourceNotFoundException("Usuario Protegido por senha");
                }
                bool logged = SecurityService.ValidarSenha(password, aluno.password);
                if (!logged)
                {
                    throw new ResourceNotFoundException("Usuario e/ou senha Incorretos!");
                }
            }
            return aluno;
        }
        catch (ResourceNotFoundException e)
        {
            Console.WriteLine(e.Message);
            return null;
        }
        catch (Exception e)
        {
            Console.WriteLine("Erro Inesperado: " + e.Message);
            return null;
        }
        
    }

    public static async Task<List<Aluno>> showAll()
    {
        return await AlunoDB.Listar();
    }
    public static async Task<NetworkPacket> enterGame()
    {
        var ipGame = new TaskCompletionSource<string>();
        NetworkPacket networkPacket = new NetworkPacket();
        Action<string, string> tratarMensagemUDP = null;
        tratarMensagemUDP = (senderIP, jsonRecebido) =>
        {
            try
            {
                var pacote = JsonSerializer.Deserialize<NetworkPacket>(jsonRecebido);
                if (pacote != null && pacote.Tipo == "DESCOBERTA_SERVIDOR")
                {
                    
                    networkPacket.Tipo = pacote.Tipo;
                    networkPacket.DadosJson = jsonRecebido;
                    ipGame.TrySetResult(senderIP);
                    BroadcastUDP.OnMessageReceived -= tratarMensagemUDP;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
        };
        BroadcastUDP.OnMessageReceived += tratarMensagemUDP;
        
        _ = Task.Run(() => BroadcastUDP.StartListeningAsync());

        string ipServer = await ipGame.Task;
        
        networkPacket.senderIP = ipServer;
        
        BroadcastUDP.StopListening();
        
        await DB.Connect(ipServer);
        
        return networkPacket;
    }
    public static async Task<Aluno> Read(int id)
    {
        Aluno aluno = await AlunoDB.GetAlunoById(id);

        if (aluno == null)
        {
            throw new ResourceNotFoundException($"Nenhuma sala foi encontrada.");
        }
        return aluno;
    }
    public static async Task SendMyPersona(int id)
    {
        var pacoteServidor = new NetworkPacket{
            Tipo = "ENTRADA",
            DadosJson = JsonSerializer.Serialize(new {
                Status = "ENTROU_SERVIDOR",
                ID = id
            })
        };
        Task.Run(async () =>
        {
            while (true)
            {
                string jsonParaEnviar = JsonSerializer.Serialize(pacoteServidor);
                BroadcastUDP.SendBroadcast(jsonParaEnviar);
                await Task.Delay(1000); // Envia a cada 1 segundo
            }
        });

    }
}