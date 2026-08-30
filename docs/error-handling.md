# Tratamento de erros

A API usa `ProblemDetails` e `IExceptionHandler`. Exceções de negócio são traduzidas para status HTTP sem que o domínio conheça HTTP.

| Exceção | Status |
|---|---:|
| `RegistroNaoEncontradoException` | 404 |
| `PermissaoNegadaException` | 403 |
| `PropriedadeObrigatoriaException` | 400 |
| `PropriedadeInvalidaException` | 422 |
| `ValorInvalidoException` | 422 |
| `OperacaoNaoPermitidaException` | 422 |
| `NegocioException` | 422 |
| Exceção desconhecida | 500 |
