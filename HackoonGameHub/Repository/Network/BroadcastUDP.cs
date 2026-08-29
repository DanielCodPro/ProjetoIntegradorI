using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Repository.Exceptions;

namespace Repository.Postgress;

public class BroadcastUDP
{
    private const int Porta = 11000; 
    
    // O evento que passa a string do JSON e o IP
    public static event Action<string, string>? OnMessageReceived;

    // Controle para garantir que não vamos abrir dois ouvintes na mesma máquina
    private static bool _estaOuvindo = false;
    
    //Função para Ler Dados Json na Porta
    public static async Task StartListeningAsync()
    {
        // Se já tiver um ouvinte rodando nesta máquina, não abre outro
        if (_estaOuvindo) return;
        _estaOuvindo = true;
        
        UdpClient? udpListener = null;

        try
        {
            udpListener = new UdpClient(Porta);
            Console.WriteLine($"[UDP] Ouvinte ativado com sucesso na porta {Porta}...");

            while (_estaOuvindo)
            {
                try
                {
                    // Aguarda a chegada de um pacote de forma assíncrona
                    var result = await udpListener.ReceiveAsync();
                    
                    string jsonMessage = Encoding.UTF8.GetString(result.Buffer);
                    string senderIP = result.RemoteEndPoint.Address.ToString();

                    // Dispara o evento de forma segura
                    OnMessageReceived?.Invoke(senderIP, jsonMessage);
                }
                catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionReset)
                {
                    // Erro comum de UDP no Windows (ignora e continua ouvindo)
                    continue;
                }
                catch (Exception)
                {
                    // Se der erro de leitura ou JSON corrompido, não mata o loop! 
                    // Apenas ignora o pacote ruim e espera o próximo
                    continue;
                }
            }
        }
        catch (Exception e)
        {
            _estaOuvindo = false;
            throw new NetworkBroadcastException("Erro fatal de inicialização do socket Broadcast", e);
        }
        finally
        {
            // Garante que se o loop parar, a porta é liberada IMEDIATAMENTE
            udpListener?.Close();
            udpListener?.Dispose();
            _estaOuvindo = false;
        }
    }

    public static void StopListening()
    {
        _estaOuvindo = false;
    }
    
    // Função que Envia os Dados na Rede local Pela Porta
    public static void SendBroadcast(object messageObject)
    {
        try
        {
            using var udpClient = new UdpClient();
            udpClient.EnableBroadcast = true;
            
            var endPoint = new IPEndPoint(IPAddress.Broadcast, Porta);
            
            string json = JsonSerializer.Serialize(messageObject);
            byte[] bytes = Encoding.UTF8.GetBytes(json);

            udpClient.Send(bytes, bytes.Length, endPoint);
        }
        catch (Exception e)
        {
            throw new NetworkBroadcastException("Falha ao disparar pacote na rede local.", e);
        }
    }
    
}