namespace Filoroch.Template.CrossCutting.Exceptions;

public sealed class OperacaoNaoPermitidaException(string message) : NegocioException(message)
{
}
