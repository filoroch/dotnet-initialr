# Testes

Somente entidades e serviços do domínio são testados. A convenção é AAA,
classes internas agrupadas por método, variável `sut`, NSubstitute,
FluentAssertions e NBuilder quando necessário.

Os nomes seguem o formato `Dado_Cenario_Espero_Comportamento`. Os testes de
serviço validam tanto o resultado quanto as interações esperadas com os
repositórios, incluindo chamadas que não devem ocorrer em cenários inválidos.

Não há Builders próprios, Fixtures, `Usings.cs` ou testes para Application, Infrastructure, Apps e IoC.
