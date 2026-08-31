using Filoroch.Template.CrossCutting.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Filoroch.Template.IoC.ExceptionHandling;

public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetails,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetails = problemDetails;
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled exception");

        var status = exception switch
        {
            AutenticacaoException => StatusCodes.Status401Unauthorized,
            RegistroNaoEncontradoException => StatusCodes.Status404NotFound,
            PermissaoNegadaException => StatusCodes.Status403Forbidden,
            PropriedadeObrigatoriaException => StatusCodes.Status400BadRequest,
            OperacaoNaoPermitidaException => StatusCodes.Status422UnprocessableEntity,
            NegocioException => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status500InternalServerError
        };

        var problem = new ProblemDetails
        {
            Status = status,
            Title = GetTitle(status),
            Detail = status == StatusCodes.Status500InternalServerError
                ? "Ocorreu um erro interno."
                : exception.Message,
            Instance = httpContext.Request.Path
        };

        problem.Extensions["traceId"] = httpContext.TraceIdentifier;
        httpContext.Response.StatusCode = status;

        await _problemDetails.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem
        });

        return true;
    }

    private static string GetTitle(int status) => status switch
    {
        StatusCodes.Status400BadRequest => "Requisição inválida",
        StatusCodes.Status401Unauthorized => "Não autenticado",
        StatusCodes.Status403Forbidden => "Acesso negado",
        StatusCodes.Status404NotFound => "Registro não encontrado",
        StatusCodes.Status422UnprocessableEntity => "Operação não permitida",
        _ => "Erro interno"
    };
}
