namespace Filoroch.Template.CrossCutting.Exceptions;

public sealed class PropriedadeInvalidaException(string propertyName, string message)
    : NegocioException($"A propriedade '{propertyName}' é inválida. {message}")
{
}
