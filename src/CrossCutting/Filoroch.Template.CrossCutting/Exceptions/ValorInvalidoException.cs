namespace Filoroch.Template.CrossCutting.Exceptions;

public sealed class ValorInvalidoException(string propertyName, string message)
    : NegocioException($"O valor da propriedade '{propertyName}' é inválido. {message}")
{
}
