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
        try
        {
            professor.password = BCrypt.Net.BCrypt.HashPassword(professor.password);
            await ProfessorDB.Create(professor);
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

    public static async Task<Professor?> LoginProfessor(string email, string password)
    {

        Professor professor;
        try
        {
            professor = await ProfessorDB.GetProfessorByEmail(email);
            bool logged = SecurityService.ValidarSenha(password, professor.password);
            if (!logged)
            {
                throw new ResourceNotFoundException("Email e/ou senha Incorretos!");
            }

            return professor;

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
    
    public static async Task StartGameAsync()
    {
        await DB.Connect();

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
    }
}