using Filoroch.Template.CrossCutting.Exceptions;
using Filoroch.Template.CrossCutting.Extensions;
using Filoroch.Template.Domain.Usuarios.Commands;
using Filoroch.Template.Domain.Usuarios.Entities;
using Filoroch.Template.Domain.Usuarios.Repositories;

namespace Filoroch.Template.Domain.Usuarios.Services;

public sealed class UsuariosService(IUsuarioRepository _repository, IPasswordService _passwordService) : IUsuariosService
{
    public async Task<Usuario> CriarAsync(CriarUsuarioCommand command, CancellationToken cancellationToken = default)
    {
        if (await _repository.ExistePorEmailAsync(command.Email, cancellationToken))
            throw new OperacaoNaoPermitidaException("Já existe um usuário com este e-mail.");

        string senha = command.Senha.ValidarObrigatoria(nameof(command.Senha));
        string senhaHash = _passwordService.Hash(senha);
        Usuario usuario = new(command.Username, command.Email, senhaHash);
        usuario.AtualizarDataModificacao();
        await _repository.AddAsync(usuario, cancellationToken);
        return usuario;
    }

    public async Task AtualizarAsync(AtualizarUsuarioCommand command, CancellationToken cancellationToken = default)
    {
        Usuario usuario = await _repository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new RegistroNaoEncontradoException("Usuário");

        if (!string.IsNullOrWhiteSpace(command.Email) && command.Email != usuario.Email)
            await ValidarEmailAsync(command.Email, cancellationToken);

        usuario.Atualizar(command.Username, command.Email);

        if (command.Senha is not null)
        {
            string senha = command.Senha.ValidarObrigatoria(nameof(command.Senha));
            usuario.AlterarSenhaHash(_passwordService.Hash(senha));
        }

        usuario.AtualizarDataModificacao();
        await _repository.UpdateAsync(usuario, cancellationToken);
    }

    private async Task ValidarEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (await _repository.ExistePorEmailAsync(email, cancellationToken))
            throw new OperacaoNaoPermitidaException("Já existe um usuário com este e-mail.");
    }
}
