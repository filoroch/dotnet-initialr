namespace Filoroch.Template.Domain.Usuarios.Services;

public interface IPasswordService
{
    string Hash(string senha);
    bool Verify(string senha, string hash);
}
