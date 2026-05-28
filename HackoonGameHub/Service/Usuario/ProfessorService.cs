using System.Text.Json;
using System.Text.Json.Serialization;
using Models;
using Repository.Exceptions;
using Repository.Postgress;

namespace Service;

public class ProfessorService
{
    public static async void RegisterProfessor(Professor professor)
    {
             professor.password = BCrypt.Net.BCrypt.HashPassword(professor.password);
            await ProfessorDB.Create(professor);
    }

    public static async Task<Professor?> LoginProfessor(string email, string password)
    {

        Professor professor;
       
            professor = await ProfessorDB.GetProfessorByEmail(email);
            bool logged = SecurityService.ValidarSenha(password, professor.password);
            if (!logged)
            {
                throw new ResourceNotFoundException("Email e/ou senha Incorretos!");
            }

            return professor;
    }
    
    public static async Task<string> StartGameAsync()
    {
        string r = await DB.Connect();

        var pacoteServidor = new NetworkPacket{
            Tipo = "DESCOBERTA_SERVIDOR",
            DadosJson = JsonSerializer.Serialize(new {Status = "LOBBY_ABERTO"})
        };
        _ = Task.Run(async () =>
        {
            while (true)
            {
                BroadcastUDP.SendBroadcast(pacoteServidor);
                await Task.Delay(1000);
            }
        });

        return r;
    }
}