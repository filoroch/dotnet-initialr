namespace Filoroch.Template.CrossCutting.Exceptions;

public sealed class AutenticacaoException(string message = "Credenciais inválidas.") : NegocioException(message)
{
}
