namespace Filoroch.Template.CrossCutting.Exceptions;

public sealed class PropriedadeObrigatoriaException(string propertyName) 
    : NegocioException($"A propriedade '{propertyName}' é obrigatória."){}
