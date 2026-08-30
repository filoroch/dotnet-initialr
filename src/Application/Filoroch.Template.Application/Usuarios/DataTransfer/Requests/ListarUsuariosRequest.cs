using Filoroch.Template.CrossCutting.Persistence.Pagination;

namespace Filoroch.Template.Application.Usuarios.DataTransfer.Requests;

public sealed class ListarUsuariosRequest : PaginationRequest
{
    public string? Nome { get; set; }
    public string? Email { get; set; }
    public bool? Ativo { get; set; }
}
