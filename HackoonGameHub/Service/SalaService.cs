using Models;
using Repository.Exceptions;
using Repository.Postgress;

namespace Service;

public class SalaService
{
    public static async Task CreateSala(Sala sala)
    {
        if (string.IsNullOrEmpty(sala.name))
        {
            throw new InvalidParameterException("Nome da Sala é Obrigatório");
        }
        await SalaDB.Create(sala);
    }

    public static async Task<List<Sala>> ReadAll()
    {
        List<Sala> sala = await SalaDB.ReadAll();

        if (sala.Count == 0)
        {
            throw new ResourceNotFoundException($"Nenhuma sala foi encontrada.");
        }
        
        return sala;
    }

    public static async Task<Sala> Read(int id)
    {
        Sala sala = await SalaDB.Read(id);

        if (sala == null)
        {
            throw new ResourceNotFoundException($"Nenhuma sala foi encontrada.");
        }
        return sala;
    }

    public static async Task<Sala> Update(Sala sala, int id)
    {
        Sala oldSala = await SalaDB.Read(id);

        if (oldSala == null)
        {
            throw new ResourceNotFoundException($"Nenhuma sala foi encontrada.");
        }
        
        if (string.IsNullOrEmpty(sala.name))
        {
            sala.name = oldSala.name;
        }

        if (string.IsNullOrEmpty(sala.descricao) && !string.IsNullOrEmpty(oldSala.descricao))
        {
            sala.descricao = oldSala.descricao;
        }
        
        await SalaDB.Update(sala, id);
        
        return sala;
    }
    public static async  Task Delete(int id)
    {
        Sala sala = await SalaDB.Read(id);
        
        if (sala == null)
        {
            throw new ResourceNotFoundException($"Sala com ID {id} não encontrada.");
        }

        await SalaDB.Delete(id);
    }
}