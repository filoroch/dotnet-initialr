namespace Filoroch.Template.CrossCutting.Exceptions;

public abstract class NegocioException(string message) : Exception(message)
{
}
