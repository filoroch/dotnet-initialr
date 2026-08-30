using Filoroch.Template.CrossCutting.Exceptions;
using Filoroch.Template.CrossCutting.Extensions;

namespace Filoroch.Template.Domain.Usuarios.Entities;

public sealed class Usuario
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public bool Ativo { get; private set; }

    protected Usuario() { }
    
    public Usuario(string nome, string email)
    {
        Id = Guid.NewGuid();
        SetNome(nome);
        SetEmail(email);
        
        Ativar();
    }

    public void Ativar() => Ativo = true;

    public void Atualizar(string nome, string email)
    {
        if (!string.IsNullOrWhiteSpace(nome) && nome != Nome)
            SetNome(nome);

        if (!string.IsNullOrWhiteSpace(email) && email != Email)
            SetEmail(email);
    }

    private void SetNome(string nome)
    {
        Nome = nome.ValidarObrigatoria(nameof(Nome), minLength: 3, maxLength: 100);
    }

    private void SetEmail(string email)
    {
        string normalizedEmail = email.ValidarObrigatoria(nameof(Email), maxLength: 254);

        if (!normalizedEmail.IsValidEmail())
            throw new ValorInvalidoException(nameof(Email), "Informe um endereço de e-mail válido.");

        Email = normalizedEmail.ToLowerInvariant();
    }

    public void Desativar() => Ativo = false;
}
