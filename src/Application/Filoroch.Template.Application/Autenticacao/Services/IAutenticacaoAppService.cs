using Filoroch.Template.Application.Autenticacao.DataTransfer.Requests;
using Filoroch.Template.Application.Autenticacao.DataTransfer.Responses;

namespace Filoroch.Template.Application.Autenticacao.Services;

public interface IAutenticacaoAppService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
