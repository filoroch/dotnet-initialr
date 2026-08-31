using FluentAssertions;
using Filoroch.Template.CrossCutting.Exceptions;
using Filoroch.Template.Domain.Usuarios.Entities;
using FluentAssertions.Specialized;

namespace Filoroch.Template.Domain.Tests.Usuarios.Entities;

public class UsuarioTests
{

    protected Usuario sut;

    public UsuarioTests()
    {
        sut = new Usuario("Filipe Rocha", "email@example.com", "hash");
    }

    public class Construtor : UsuarioTests
    {
        [Fact]
        public void Dado_DadosValidos_Espero_CriarUsuario()
        {
            sut = new ("Filipe Rocha", "FILIPE@EMAIL.COM", "hash");

            sut.Username.Should().Be("Filipe Rocha");
            sut.Email.Should().Be("filipe@email.com");
            sut.Ativo.Should().BeTrue();
        } 
    }

    public class Atualizar : UsuarioTests
    {
        [Fact]
        public void Dado_DadosValidos_Espero_AtualizarUsuario()
        {
            sut.Atualizar("Novo Nome", "novoemail@example.com");

            sut.Username.Should().Be("Novo Nome");
            sut.Email.Should().Be("novoemail@example.com");
        }

        [Fact]
        public void Dado_EmailInvalido_Espero_LancarExcecao()
        {
            Action act = () => sut.Atualizar("Novo Nome", "novoemail@");

            var ex = act.Should().Throw<ValorInvalidoException>();

            ex.WithMessage("O valor da propriedade 'Email' é inválido. Informe um endereço de e-mail válido.");
        }

        [Fact]
        public void Dado_NomeInvalido_Espero_LancarExcecao()
        {
            Action act = () => sut.Atualizar("f2","novoemail@mail.com");

            var ex = act.Should().Throw<PropriedadeInvalidaException>();

            ex.WithMessage("A propriedade 'Username' é inválida. deve possuir no mínimo 3 caracteres.");
        }
    }
}
