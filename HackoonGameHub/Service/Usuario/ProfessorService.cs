using System.Text.Json;
using System.Text.Json.Serialization;
using Models;
using Repository.Exceptions;
using Repository.Postgress;

namespace Service;

public static class ProfessorService
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

    public static async Task SendMessage(string message)
    {
        var pacoteServidor = new NetworkPacket{
            Tipo = "DESCOBERTA_SERVIDOR",
            DadosJson = JsonSerializer.Serialize(new {
                Status = "LOBBY_ABERTO",
                Message = message
            })
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
    public static async Task<string> StartGameAsync()
    {
        string resultado = await DB.Connect();
        
        return resultado;
    }

    public static async Task<Professor> Update(Professor professor)
    {
        if (professor.id < 0)
        {
            throw new ResourceNotFoundException("Usuario não logaddo");
        }
        
        Professor oldProfessor = await ProfessorDB.GetProfessorById(professor.id);
        
        if (oldProfessor == null)
        {
            throw new ResourceNotFoundException("Não existe Cadastro No banco");
        }

        if (string.IsNullOrEmpty(professor.email))
        {
            professor.email = oldProfessor.email;
        }

        if (string.IsNullOrEmpty(professor.email))
        {
            professor.email = oldProfessor.email;
        }

        if (string.IsNullOrEmpty(professor.password))
        {
            professor.password = oldProfessor.password;
        }
        else
        {
            professor.password = SecurityService.GerarHaskSenha(professor.password);
        }
        
        await ProfessorDB.Update(professor, professor.id);

        return professor;

    }
    
}