namespace Models;

public class NetworkPacket
{
    public string Tipo { get; set; } = string.Empty; // EX: "DESCOBERTA_SERVIDOR", "PERMISSAO_NOME", "STATUS_ENTRADA"
    public string DadosJson { get; set; } = string.Empty; // Aqui dentro vai QUALQUER sub-JSON em string
}