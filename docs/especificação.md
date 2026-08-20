# Especificação técnica

## 1. Objetivo

Este documento resume as tecnologias, bibliotecas, padrões e práticas adotados na solução do desafio técnico. Ele descreve o estado atual do repositório; itens ainda não implementados são identificados explicitamente.

## 2. Tecnologias utilizadas

### 2.1. Backend

| Tecnologia | Versão | Finalidade |
|---|---:|---|
| .NET | 10 | Plataforma dos microsserviços e dos testes |
| Entity Framework Core | 10.0.11 | Mapeamento objeto-relacional, consultas LINQ, transações e migrations |
| EF Core SQL Server | 10.0.11 | Provider de persistência para SQL Server |
| SQL Server | 2022 | Banco relacional real e independente para cada microsserviço |
| FluentValidation | 12.1.1 | Validação dos contratos de entrada na camada Application |
| OpenAPI | Microsoft.AspNetCore.OpenApi 10.0.11 | Geração do contrato das APIs |
| Scalar | 2.16.20 | Interface visual da documentação OpenAPI |
| ASP.NET API Versioning | 10.2.x | Versionamento das rotas e documentos OpenAPI |
| Serilog.AspNetCore | 10.0.0 | Logging estruturado e integração com `ILogger<T>` |
| Serilog.Sinks.MSSqlServer | 10.0.0 | Persistência de eventos de erro no SQL Server de cada serviço |

### 2.2. Testes

| Tecnologia | Versão | Finalidade |
|---|---:|---|
| xUnit | 2.9.3 | Framework de testes unitários e de integração |
| Shouldly | 4.3.0 | Asserções legíveis |
| Microsoft.NET.Test.Sdk | 18.9.0 | Descoberta e execução dos testes |
| Testcontainers for .NET | 4.13.0 | Criação de SQL Server descartável nos testes de integração |

Os testes de integração não utilizam o provider EF Core InMemory. As regras que dependem de constraint, transação, SQL condicional, idempotência e concorrência são verificadas contra SQL Server real.

### 2.3. Infraestrutura local

| Tecnologia | Finalidade |
|---|---|
| Docker | Execução dos containers SQL Server e dos testes com Testcontainers |

## 3. Organização da solução

A solução contém dois microsserviços independentes:

- `Estoque`: produtos, saldos, consulta de produtos e baixa de estoque;
- `Faturamento`: notas, itens, numeração, estado e fechamento.

Cada microsserviço é separado nos seguintes projetos:

- `Api`: transporte HTTP, composição da aplicação e tradução de resultados para respostas HTTP;
- `Application`: casos de uso, validação, contratos e portas;
- `Domain`: entidades, invariantes, erros e tipos de domínio;
- `Infrastructure`: EF Core, SQL Server, repositórios e integrações HTTP;
- `UnitTests`: testes isolados de domínio e casos de uso;
- `IntegrationTests`: testes com persistência real.

## 4. Padrões arquiteturais e de código

### 4.1. Microsserviços e banco por serviço

Cada serviço possui seu próprio `DbContext`, migrations e banco SQL Server. Nenhum serviço consulta diretamente o banco do outro. A integração entre Faturamento e Estoque ocorre exclusivamente por HTTP/JSON.

### 4.2. Clean Architecture pragmática

A direção de dependências adotada é:

```text
API -> Application -> Domain
 |          ^
 v          |
Infrastructure
```

O domínio não depende de ASP.NET Core, EF Core, HTTP ou SQL Server. A Infrastructure implementa contratos declarados pela Application.

### 4.3. Domain-Driven Design pragmático

Foram utilizados conceitos de DDD adequados ao tamanho do desafio:

- entidades com comportamento: `Product`, `StockOperation`, `Invoice` e `InvoiceItem`;
- invariantes protegidas dentro das entidades;
- agregados sem setters públicos para estado crítico;
- linguagem de negócio explícita;
- erros de domínio tipados;
- separação dos contextos de Estoque e Faturamento.

Não foram adicionados Domain Events, Event Sourcing ou CQRS completo porque não existe necessidade concreta no escopo.

### 4.4. Organização por feature/use case

Na Application, requests, responses, validators e handlers são agrupados por caso de uso, como `CreateProduct`, `DebitStock`, `CreateInvoice` e `CloseInvoice`.

### 4.5. Result Pattern

Os casos de uso retornam `Result<T>` para representar sucesso ou falha esperada sem usar exceções como fluxo normal. A API converte esses resultados em respostas HTTP e `ProblemDetails`.

### 4.6. FluentValidation

Os validators protegem o contrato de entrada, incluindo campos obrigatórios, `Guid.Empty`, coleções vazias, quantidades inválidas e produtos repetidos. As validações não substituem as invariantes do domínio nem as constraints do banco.

### 4.7. Repositórios específicos

Foram criadas portas direcionadas às necessidades dos casos de uso, como `IProductRepository`, `IStockDebitRepository` e `IInvoiceRepository`. Não foi adotado um repositório genérico ou uma Unit of Work abstrata, pois o `DbContext` já oferece a unidade transacional necessária.

### 4.8. Gateway/Ports and Adapters

`IInventoryGateway` é a porta usada pelo Faturamento para consultar produtos e solicitar baixas ao Estoque. A implementação HTTP fica na Infrastructure e não expõe tipos HTTP ao Domain.

### 4.9. Injeção de dependência

A composição utiliza o container nativo do ASP.NET Core. Cada camada possui extensões de registro próprias e as dependências são recebidas por construtor.

### 4.10. Options e configuração por ambiente

Connection strings, CORS, rate limiting, endereço e timeout da API de Estoque são configuráveis. Segredos de produção não ficam nos arquivos base. Em desenvolvimento, as migrations automáticas verificam primeiro se existem migrations pendentes.

### 4.11. Problem Details e middleware de exceções

Erros são retornados como `application/problem+json`, com status HTTP, título, detalhe, código estável e `traceId` quando aplicável. Um middleware central traduz falhas conhecidas e impede a exposição de stack traces.

### 4.12. Idempotência

O fechamento usa a chave estável `invoice:{invoiceId}:close:v1`. O Estoque persiste a chave, o identificador da nota e o hash canônico do payload. Repetições iguais não baixam novamente; reutilização da chave com conteúdo diferente gera conflito.

### 4.13. Transação e concorrência

A baixa de todos os itens ocorre em uma única transação local do Estoque. Os itens são ordenados por `ProductId` e o saldo é atualizado por SQL condicional (`saldo >= quantidade`). Se qualquer item falhar, a transação é revertida. Essa estratégia impede saldo negativo e baixa parcial sob concorrência.

### 4.14. Numeração por sequence

O Faturamento utiliza a sequence SQL Server `invoice_number_sequence` para gerar números únicos e sequenciais no backend. Lacunas são aceitas; duplicidades não.

### 4.15. LINQ

LINQ é utilizado nas consultas EF Core para filtros, ordenação, projeção, paginação e atualizações condicionais, além de validações e transformações em memória. `IQueryable` permanece restrito à Infrastructure.

### 4.16. Async/await e cancelamento

Operações de I/O são assíncronas e propagam `CancellationToken`. O código não utiliza `.Result` ou `.Wait()` no fluxo das requisições.

### 4.17. Migrations

Cada microsserviço mantém suas próprias migrations. Na inicialização local, `GetPendingMigrationsAsync()` é consultado e `MigrateAsync()` só é chamado quando existe ao menos uma migration pendente.

### 4.18. Testes

Os testes seguem o formato comportamento–cenário–resultado. Os unitários cobrem invariantes e handlers; os testes de integração verificam SQL Server, constraints, sequence, atomicidade, rollback, idempotência, falha entre serviços e concorrência.

### 4.19. Logging estruturado

Todos os casos de uso utilizam `ILogger<T>` para registrar início, sucesso e rejeições com propriedades estruturadas. Serilog centraliza os logs das duas APIs:

- eventos a partir de `Information` são enviados ao console;
- falhas de negócio esperadas são registradas como `Warning`;
- exceções e indisponibilidades técnicas são registradas como `Error`;
- eventos `Error` ou `Fatal` são persistidos na tabela `error_logs` do banco pertencente ao próprio microsserviço;
- a tabela é criada automaticamente pelo sink quando o primeiro erro precisa ser gravado;
- cada evento recebe a propriedade `Service`, com o valor `Estoque` ou `Faturamento`.

Não são registrados corpos completos de requisição, connection strings, senhas ou outros segredos.

## 5. Recursos nativos priorizados

A solução prioriza recursos nativos da plataforma:

- injeção de dependência do ASP.NET Core;
- `ILogger<T>`;
- `TimeProvider` em fluxos dependentes de tempo;
- `HttpClientFactory`;
- `ProblemDetails`;
- CORS;
- rate limiting;
- OpenAPI;
- EF Core migrations e transações.

Não foram adicionados MediatR, AutoMapper, mensageria, API Gateway, framework de repository ou biblioteca de resiliência.
