namespace Service;

public class SecurityService
{
    public static string GerarHaskSenha(string senha)
    {
        string senhaHash = BCrypt.Net.BCrypt.HashPassword(senha);
        return senhaHash;
    }

    public static bool ValidarSenha(string senha, string hash)
    {
        bool senhaCorreta = BCrypt.Net.BCrypt.Verify(senha, hash);
        return senhaCorreta;
    }
}