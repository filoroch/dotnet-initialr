using FluentAssertions;
using Filoroch.Template.CrossCutting.Exceptions;
using Filoroch.Template.Domain.Usuarios.Commands;
using Filoroch.Template.Domain.Usuarios.Entities;
using Filoroch.Template.Domain.Usuarios.Repositories;
using Filoroch.Template.Domain.Usuarios.Services;
using NSubstitute;

namespace Filoroch.Template.Domain.Tests.Usuarios.Services;

public class UsuarioServiceTests
{
    protected readonly IUsuarioRepository usuarioRepository;
    protected readonly IPasswordService passwordService;
    protected readonly UsuariosService sut;

    public UsuarioServiceTests()
    {
        usuarioRepository = Substitute.For<IUsuarioRepository>();
        passwordService = Substitute.For<IPasswordService>();
        passwordService.Hash(Arg.Any<string>()).Returns("hash");
        sut = new UsuariosService(usuarioRepository, passwordService);
    }

    public class CriarAsync : UsuarioServiceTests
    {
        [Fact]
        public async Task Dado_DadosValidos_Espero_CriarEAdicionarUsuario()
        {
            usuarioRepository
                .ExistePorEmailAsync("filipe@email.com", Arg.Any<CancellationToken>())
                .Returns(false);

            Usuario result = await sut.CriarAsync(
                new CriarUsuarioCommand("Filipe Rocha", "filipe@email.com", "Senha123!"));

            result.Username.Should().Be("Filipe Rocha");
            result.Email.Should().Be("filipe@email.com");
            result.Ativo.Should().BeTrue();

            await usuarioRepository.Received(1).AddAsync(
                Arg.Is<Usuario>(usuario => usuario.Id == result.Id),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Dado_EmailJaCadastrado_Espero_LancarExcecaoENaoAdicionar()
        {
            usuarioRepository
                .ExistePorEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(true);

            Func<Task<Usuario>> action = () => sut.CriarAsync(
                new CriarUsuarioCommand("Filipe Rocha", "filipe@email.com", "Senha123!"));

            await action.Should().ThrowAsync<OperacaoNaoPermitidaException>()
                .WithMessage("Já existe um usuário com este e-mail.");

            await usuarioRepository.DidNotReceive().AddAsync(
                Arg.Any<Usuario>(), Arg.Any<CancellationToken>());
        }
    }

    public class AtualizarAsync : UsuarioServiceTests
    {
        [Fact]
        public async Task Dado_DadosValidos_Espero_AtualizarUsuario()
        {
            Usuario usuario = new("Nome antigo", "antigo@email.com", "hash");
            usuarioRepository.GetByIdAsync(usuario.Id, Arg.Any<CancellationToken>())
                .Returns(usuario);
            usuarioRepository.ExistePorEmailAsync("novo@email.com", Arg.Any<CancellationToken>())
                .Returns(false);

            await sut.AtualizarAsync(new AtualizarUsuarioCommand(
                usuario.Id, "Nome novo", "novo@email.com"));

            usuario.Username.Should().Be("Nome novo");
            usuario.Email.Should().Be("novo@email.com");

            await usuarioRepository.Received(1).UpdateAsync(
                Arg.Is<Usuario>(item => item.Id == usuario.Id
                    && item.Username == "Nome novo"
                    && item.Email == "novo@email.com"),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Dado_UsuarioNaoEncontrado_Espero_LancarExcecao()
        {
            Guid id = Guid.NewGuid();
            usuarioRepository.GetByIdAsync(id, Arg.Any<CancellationToken>())
                .Returns((Usuario?)null);

            Func<Task> action = () => sut.AtualizarAsync(
                new AtualizarUsuarioCommand(id, "Nome", "email@email.com"));

            await action.Should().ThrowAsync<RegistroNaoEncontradoException>()
                .WithMessage("Registro Usuário não encontrado");

            await usuarioRepository.DidNotReceive().UpdateAsync(
                Arg.Any<Usuario>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Dado_EmailJaCadastrado_Espero_LancarExcecaoENaoAtualizar()
        {
            Usuario usuario = new("Nome", "atual@email.com", "hash");
            usuarioRepository.GetByIdAsync(usuario.Id, Arg.Any<CancellationToken>())
                .Returns(usuario);
            usuarioRepository.ExistePorEmailAsync("ocupado@email.com", Arg.Any<CancellationToken>())
                .Returns(true);

            Func<Task> action = () => sut.AtualizarAsync(new AtualizarUsuarioCommand(
                usuario.Id, "Novo nome", "ocupado@email.com"));

            await action.Should().ThrowAsync<OperacaoNaoPermitidaException>()
                .WithMessage("Já existe um usuário com este e-mail.");

            usuario.Username.Should().Be("Nome");
            usuario.Email.Should().Be("atual@email.com");
            await usuarioRepository.DidNotReceive().UpdateAsync(
                Arg.Any<Usuario>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Dado_NomeEEmailNaoInformados_Espero_ManterDadosEAtualizarRepositorio()
        {
            Usuario usuario = new("Nome", "email@email.com", "hash");
            usuarioRepository.GetByIdAsync(usuario.Id, Arg.Any<CancellationToken>())
                .Returns(usuario);

            await sut.AtualizarAsync(new AtualizarUsuarioCommand(usuario.Id, "", ""));

            usuario.Username.Should().Be("Nome");
            usuario.Email.Should().Be("email@email.com");
            await usuarioRepository.Received(1).UpdateAsync(
                Arg.Is<Usuario>(item => item.Id == usuario.Id),
                Arg.Any<CancellationToken>());
            await usuarioRepository.DidNotReceive().ExistePorEmailAsync(
                Arg.Any<string>(), Arg.Any<CancellationToken>());
        }
    }
}
