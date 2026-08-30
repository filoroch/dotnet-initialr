namespace Filoroch.Template.CrossCutting.Exceptions;

public sealed class RegistroNaoEncontradoException : NegocioException
{
    public RegistroNaoEncontradoException()
        : base("Registro não encontrado") { }

    public RegistroNaoEncontradoException(string propertyType) 
        : base($"Registro {propertyType} não encontrado"){}

    public  RegistroNaoEncontradoException(int propertyId) 
        : base($"Registro de id: {propertyId} não encontrado"){}
}
