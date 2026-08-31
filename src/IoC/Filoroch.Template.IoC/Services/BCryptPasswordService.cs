using Filoroch.Template.Domain.Usuarios.Services;

namespace Filoroch.Template.IoC.Services;

public sealed class BCryptPasswordService : IPasswordService
{
    public bool Verify(string senha, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(senha, hash);
        }
        catch (Exception)
        {
            return false;
        }
    }

    public string Hash(string senha) => BCrypt.Net.BCrypt.HashPassword(senha);
}
