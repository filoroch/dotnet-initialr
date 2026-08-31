using Filoroch.Template.Application.Usuarios.DataTransfer.Requests;
using Filoroch.Template.Application.Usuarios.DataTransfer.Responses;
using Filoroch.Template.Application.Usuarios.Services;
using Filoroch.Template.CrossCutting.Persistence.Pagination;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace Filoroch.Template.Mcp.Usuarios;

[McpServerToolType]
public sealed class UsuarioTools(
    IUsuarioAppService usuarioAppService,
    ILogger<UsuarioTools> logger)
{
    [McpServerTool(Name = "criar_usuario")]
    [Description("Cria um usuário ativo na plataforma, validando nome e e-mail conforme as regras do contexto de usuários.")]
    public async Task<UsuarioResponse> CriarUsuario(
        [Description("Username do usuário, com 3 a 100 caracteres.")] string username,
        [Description("Endereço de e-mail único e válido do usuário.")] string email,
        [Description("Senha do usuário.")] string senha,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Iniciando criação de usuário pelo MCP");

        UsuarioResponse response = await usuarioAppService.CriarAsync(
            new CriarUsuarioRequest { Username = username, Email = email, Senha = senha },
            cancellationToken);

        logger.LogInformation("Usuário criado pelo MCP: {UsuarioId}", response.Id);
        return response;
    }

    [McpServerTool(Name = "listar_usuarios")]
    [Description("Lista usuários da plataforma com filtros opcionais e paginação.")]
    public async Task<PaginatedResult<UsuarioQueryResponse>> ListarUsuarios(
        [Description("Filtra pelo username ou parte do username.")] string? usernameFilter = null,
        [Description("Filtra pelo e-mail ou parte do e-mail.")] string? email = null,
        [Description("Filtra pelo status ativo (true) ou inativo (false).")] bool? ativo = null,
        [Description("Quantidade de registros por página (padrão: 20). Use um valor positivo.")] int quantidade = 20,
        [Description("Número da página, começando em 1.")] int pagina = 1,
        CancellationToken cancellationToken = default)
    {
        if (quantidade <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantidade), "A quantidade deve ser maior que zero.");

        if (pagina <= 0)
            throw new ArgumentOutOfRangeException(nameof(pagina), "A página deve ser maior que zero.");

        logger.LogInformation(
            "Listando usuários pelo MCP. Página: {Pagina}, Quantidade: {Quantidade}, FiltroAtivoInformado: {FiltroAtivoInformado}",
            pagina, quantidade, ativo.HasValue);

        PaginatedResult<UsuarioQueryResponse> response = await usuarioAppService.ListarAsync(
            new ListarUsuariosRequest
            {
                Username = usernameFilter,
                Email = email,
                Ativo = ativo,
                Quantity = quantidade,
                Page = pagina
            },
            cancellationToken);

        logger.LogInformation(
            "Listagem de usuários concluída pelo MCP. Página: {Pagina}, Itens: {QuantidadeItens}, Total: {TotalItens}",
            pagina, response.Items.Count, response.TotalItems);

        return response;
    }
}
