namespace Filoroch.Template.CrossCutting.Exceptions;

public sealed class PermissaoNegadaException(string message = "Permissão negada.") : NegocioException(message)
{
}
