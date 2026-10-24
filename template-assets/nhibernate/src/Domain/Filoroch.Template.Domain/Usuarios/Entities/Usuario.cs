using Filoroch.Template.CrossCutting.Exceptions;
using Filoroch.Template.CrossCutting.Extensions;
using Filoroch.Template.Domain.Usuarios.Enums;

namespace Filoroch.Template.Domain.Usuarios.Entities;

public sealed class Usuario
{
    public Guid Id { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public bool Ativo { get; private set; }
    public string SenhaHash { get; private set; } = string.Empty;
    public PerfilUsuario Perfil { get; private set; } = PerfilUsuario.User;
    public DateTime CriadoEm { get; private set; }
    public DateTime AtualizadoEm { get; private set; }

    protected Usuario() { }
    
    public Usuario(string username, string email, string senhaHash, PerfilUsuario perfil = PerfilUsuario.User)
    {
        Id = Guid.NewGuid();
        SetUsername(username);
        SetEmail(email);
        SetSenhaHash(senhaHash);
        Perfil = perfil;
        CriadoEm = DateTime.UtcNow;
        AtualizadoEm = CriadoEm;
        
        Ativar();
    }

    public void Ativar() => Ativo = true;

    public void Atualizar(string username, string email)
    {
        if (!string.IsNullOrWhiteSpace(username) && username != Username)
            SetUsername(username);

        if (!string.IsNullOrWhiteSpace(email) && email != Email)
            SetEmail(email);
    }

    public void AlterarSenhaHash(string senhaHash) => SetSenhaHash(senhaHash);

    public void AtualizarDataModificacao() => AtualizadoEm = DateTime.UtcNow;

    private void SetUsername(string username)
    {
        Username = username.ValidarObrigatoria(nameof(Username), minLength: 3, maxLength: 100);
    }

    private void SetSenhaHash(string senhaHash)
        => SenhaHash = senhaHash.ValidarObrigatoria(nameof(SenhaHash));

    private void SetEmail(string email)
    {
        string normalizedEmail = email.ValidarObrigatoria(nameof(Email), maxLength: 254);

        if (!normalizedEmail.IsValidEmail())
            throw new ValorInvalidoException(nameof(Email), "Informe um endereço de e-mail válido.");

        Email = normalizedEmail.ToLowerInvariant();
    }

    public void Desativar() => Ativo = false;
}
