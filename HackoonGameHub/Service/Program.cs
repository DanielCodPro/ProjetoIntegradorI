using Models;

namespace Service;

public class Program
{
    public static async Task Main(string[] args)
    {
        await ProfessorService.StartGameAsync();
        /*
        Professor professor = new Professor("Francisco", "franciscoyuri145@gmail.com");
        
        professor.password = "franciscoyuri145";
        
        ProfessorService.CadastrarProfessor(professor);
        */
        Professor? professor = await ProfessorService.LoginProfessor("franciscoyuri145@gmail.com" , "franciscoyuri145");
        if (professor is not null)
        {
            Console.Write("Professor Logado");
        }
        /*
        Aluno aluno = new Aluno("Chicolino", "Francisco");
        aluno.password = "franciscoyuri145";

        AlunoService.RegisterAluno(aluno);
        */
        Aluno? aluno = await AlunoService.LoginAluno("Chicolino", "franciscoyuri1456");
        if (aluno is not null)
        {
            Console.Write("Aluno Logado");
        }
    }
}