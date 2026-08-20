# Detalhamento técnico da solução

## 1. Identificação do projeto

- **Projeto:** Sistema simplificado de emissão de notas fiscais
- **Repositório:** [Korp_Teste_HumbertoGuedes](https://github.com/FHumberto/Korp_Teste_HumbertoGuedes)
- **Frontend:** Angular 22
- **Backend:** .NET 10 e ASP.NET Core
- **Persistência:** SQL Server, com um banco exclusivo para cada microsserviço
- **Execução integrada:** Docker Compose

## 2. Visão geral da solução

A solução permite cadastrar produtos, consultar seus saldos, criar notas com vários itens e emitir uma nota. A emissão representa a operação de fechamento: o Faturamento solicita ao Estoque a baixa das quantidades e somente marca a nota como fechada depois que o Estoque confirma o processamento.

O sistema foi dividido em dois microsserviços:

- **Estoque:** proprietário dos produtos, saldos e operações de baixa;
- **Faturamento:** proprietário das notas, itens, numeração sequencial, estado e fechamento.

Cada microsserviço possui seu próprio banco SQL Server. Não existem joins, chaves estrangeiras ou acesso direto entre os bancos. A comunicação entre Faturamento e Estoque ocorre de forma síncrona por HTTP/JSON, por meio de contratos versionados em `/api/v1`.

```text
Angular
  |-- HTTP --> Estoque.Api ------> SQL Server do Estoque
  |
  `-- HTTP --> Faturamento.Api --> SQL Server do Faturamento
                    |
                    `-- HTTP --> Estoque.Api
```

## 3. Funcionalidades implementadas

### 3.1. Produtos

- cadastro com código, descrição e saldo inicial;
- validação de campos obrigatórios e limites de tamanho;
- rejeição de saldo inicial negativo ou fracionário;
- garantia de código único;
- listagem paginada de produtos e saldos;
- consulta individual de produto;
- listagem dos produtos com saldo positivo para composição de notas;
- persistência física no banco exclusivo do Estoque.

### 3.2. Notas

- criação de nota com um ou mais produtos;
- bloqueio de produtos repetidos e quantidades inválidas;
- geração do número sequencial no backend por uma sequence do SQL Server;
- criação sempre no estado `Open`;
- armazenamento de código e descrição do produto como snapshot do item;
- listagem de notas, com filtro opcional por estado;
- consulta dos detalhes e itens da nota;
- emissão e fechamento da nota;
- geração sob demanda do PDF de uma nota fechada;
- nova visualização do documento sem nova baixa de estoque.

O navegador envia ao Faturamento somente o identificador do produto e a quantidade. O Faturamento consulta diretamente o Estoque para obter código e descrição confiáveis, evitando confiar em dados descritivos enviados pelo cliente.

### 3.3. Fechamento e impressão

Na tela de detalhes, uma nota aberta apresenta a ação **“Emitir e imprimir”**. Durante a operação, a interface exibe processamento e desabilita a ação para evitar cliques repetidos.

O fluxo é:

1. o Faturamento carrega a nota e confirma que ela está aberta;
2. cria a chave estável `invoice:{invoiceId}:close:v1`;
3. solicita ao Estoque a baixa de todos os itens;
4. o Estoque processa a baixa em uma transação local;
5. somente após a confirmação do Estoque, o Faturamento fecha e persiste a nota;
6. o frontend consulta e abre o PDF da nota fechada.

Se a baixa for rejeitada ou o Estoque estiver indisponível, a nota permanece aberta e pode ser emitida novamente depois da correção do problema.

## 4. Arquitetura do backend

Os dois serviços seguem uma Clean Architecture pragmática, com projetos físicos para:

- **Domain:** entidades, invariantes e tipos de domínio;
- **Application:** casos de uso, validações e portas para persistência ou integração;
- **Infrastructure:** Entity Framework Core, SQL Server, geração de PDF e cliente HTTP;
- **API:** controllers, composição de dependências, configuração e tradução para HTTP.

A direção principal das dependências é:

```text
API -> Application -> Domain
 |          ^
 v          |
Infrastructure
```

O domínio não depende de ASP.NET Core, Entity Framework Core, HTTP ou SQL Server. A organização da Application é feita por feature e caso de uso, evitando serviços genéricos e abstrações sem consumidor concreto.

## 5. Ciclos de vida utilizados no Angular

Foram utilizados explicitamente os seguintes hooks:

- **`ngOnInit`:** no componente `FeedbackMessage`, para iniciar o temporizador que oculta automaticamente mensagens de sucesso após cinco segundos;
- **`ngOnDestroy`:** no mesmo componente, para cancelar o temporizador pendente e evitar que ele continue após a destruição do componente.

Nos componentes que executam requisições HTTP, o encerramento das subscriptions é associado ao ciclo de destruição por `DestroyRef` e `takeUntilDestroyed`. Essa abordagem dispensa a implementação manual de `ngOnDestroy` apenas para manter um `Subject` de cancelamento.

As páginas carregam seus dados na criação da instância quando isso é necessário. O detalhe da nota utiliza um `effect` para reagir à entrada `invoiceId` e recarregar a nota quando esse identificador muda. Hooks não foram adicionados apenas para demonstração; cada uso possui uma necessidade concreta.

## 6. Uso de RxJS

Sim. O Angular usa RxJS 7.8.2 em conjunto com `HttpClient`.

Os principais usos são:

- `Observable` como retorno dos serviços HTTP de produtos e notas;
- `subscribe` para tratar sucesso e erro das operações iniciadas pelo usuário;
- `finalize` para restaurar os estados de carregamento, envio, fechamento e consulta do documento, tanto em sucesso quanto em erro;
- `takeUntilDestroyed` para encerrar subscriptions quando o componente é destruído;
- `expand` e `reduce` para percorrer as páginas da API de produtos quando é necessária uma coleção completa.

O estado local da interface é mantido com Angular Signals (`signal` e `computed`). RxJS fica concentrado nos fluxos assíncronos e HTTP, sem a inclusão de uma biblioteca de estado global.

## 7. Bibliotecas do frontend

### 7.1. Bibliotecas de execução

| Biblioteca | Finalidade |
|---|---|
| Angular 22 | Framework da aplicação web e dos componentes standalone. |
| Angular Router | Navegação entre produtos, notas e detalhes. |
| Angular HttpClient | Comunicação com as APIs de Estoque e Faturamento. |
| Angular Forms com Signals | Formulários e validações reativas de produtos e notas. |
| RxJS 7.8.2 | Composição e controle dos fluxos HTTP assíncronos. |
| tslib | Funções auxiliares geradas pelo TypeScript. |

### 7.2. Componentes visuais e estilos

Não foi utilizada uma biblioteca pronta de componentes visuais, como Angular Material, PrimeNG ou Bootstrap. Os componentes da interface foram desenvolvidos no próprio projeto com HTML semântico, componentes Angular e Tailwind CSS 4.3.3.

O Tailwind CSS é utilizado para layout, espaçamento, tipografia, cores, estados visuais e responsividade. O uso de elementos nativos, como `dialog`, também permite controlar modal, foco e navegação por teclado sem adicionar outra biblioteca de UI.

### 7.3. Ferramentas de desenvolvimento e testes

| Ferramenta | Finalidade |
|---|---|
| TypeScript 6 | Tipagem e compilação do código do frontend. |
| Angular CLI e `@angular/build` | Desenvolvimento e build da aplicação. |
| Vitest | Execução dos testes unitários do frontend. |
| jsdom | Ambiente DOM para os testes. |
| Prettier | Padronização de formatação. |
| PostCSS | Processamento do CSS e integração do Tailwind. |

## 8. Gerenciamento de dependências

### 8.1. Frontend

As dependências do Angular são gerenciadas pelo npm. As versões diretas estão fixadas no `package.json`, e o `package-lock.json` registra toda a árvore resolvida para tornar a instalação reproduzível. O comando adotado é `npm ci`.

O projeto define Node.js 24.19.0 e npm 11.17.0 em `engines`, além de manter um arquivo `.nvmrc` para facilitar a seleção da versão do Node.js.

### 8.2. Backend C#

As dependências .NET são gerenciadas por NuGet por meio dos arquivos `.csproj`. O SDK e os projetos têm como alvo `net10.0`. A restauração é feita com `dotnet restore Korp.slnx`.

## 9. Frameworks e bibliotecas do backend C#

| Framework ou biblioteca | Finalidade |
|---|---|
| .NET 10 e ASP.NET Core | Hospedagem das APIs, controllers, DI, configuração, Problem Details, CORS e rate limiting. |
| Entity Framework Core 10 | Mapeamento, consultas, migrations e persistência. |
| Microsoft.EntityFrameworkCore.SqlServer | Provider do SQL Server. |
| FluentValidation | Validação dos contratos de entrada na camada Application. |
| Asp.Versioning | Versionamento das APIs e integração com a documentação. |
| OpenAPI e Scalar | Especificação OpenAPI e interface navegável de consulta das APIs. |
| Serilog | Logging estruturado no console e persistência de erros no SQL Server do próprio serviço. |
| QuestPDF | Geração do documento PDF da nota fechada. |
| xUnit | Framework de testes do backend. |
| Shouldly | Assertions legíveis nos testes. |
| Testcontainers for .NET | Inicialização de SQL Server real e descartável nos testes de integração. |

Não foram adicionados MediatR, AutoMapper, repository genérico ou uma abstração adicional de Unit of Work. Os casos de uso são chamados diretamente e os mapeamentos simples são explícitos.

## 10. Uso de LINQ no C#

Sim. LINQ é utilizado de forma pontual tanto em memória quanto em consultas traduzidas pelo Entity Framework Core.

Exemplos:

- `Any` para impedir a inclusão do mesmo produto duas vezes em uma nota;
- `Select` para mapear entidades e itens para responses ou comandos de integração;
- `Where` para aplicar filtros de estado e saldo;
- `OrderBy` e `ThenBy` para ordenar produtos e operações de baixa;
- `OrderByDescending` para listar as notas mais recentes primeiro;
- `Distinct` e `Count` nas validações de identificadores repetidos;
- `GroupBy` e `ToDictionary` para agrupar mensagens de validação por campo;
- `Contains` para consultar um conjunto de produtos por identificador;
- `AnyAsync`, `SingleOrDefaultAsync` e `ToListAsync` nas consultas assíncronas do Entity Framework Core.

Na baixa de estoque, os itens são ordenados por `ProductId` antes das atualizações, reduzindo o risco de deadlock quando operações concorrentes atingem os mesmos produtos.

## 11. Tratamento de erros e exceções no backend

Os erros esperados são representados por resultados da Application e convertidos pela API para respostas no padrão `application/problem+json`.

As respostas incluem, conforme o caso:

- status HTTP;
- título e detalhe em português;
- código estável do erro;
- `traceId` para correlação;
- erros de validação agrupados por campo.

Mapeamentos principais:

| HTTP | Exemplos |
|---:|---|
| 400 | contrato inválido, item vazio, quantidade inválida ou identificador repetido; |
| 404 | produto ou nota inexistente; |
| 409 | código duplicado, nota já fechada, saldo insuficiente ou conflito idempotente; |
| 429 | limite local de requisições excedido; |
| 503 | Estoque indisponível ou timeout durante uma operação do Faturamento; |
| 500 | falha inesperada ou falha de geração do documento. |

Validators do FluentValidation protegem o contrato antes dos efeitos do caso de uso, enquanto as entidades continuam protegendo suas invariantes. Exceções inesperadas são interceptadas por um `IExceptionHandler`, registradas com `ILogger`/Serilog e devolvidas sem stack trace ou detalhes internos.

O cliente HTTP do Faturamento diferencia rejeições conhecidas do Estoque e converte falhas de conexão ou timeout em `INVENTORY_UNAVAILABLE`. O `CancellationToken` é propagado pelos controllers, casos de uso, Entity Framework Core e cliente HTTP.

## 12. Persistência e integridade dos dados

O Estoque e o Faturamento utilizam bancos SQL Server independentes e migrations próprias.

No Estoque:

- `products` possui índice único para `code` e restrição para `balance >= 0`;
- `stock_operations` possui índice único para a chave de idempotência;
- a baixa de todos os itens e o registro idempotente pertencem à mesma transação.

No Faturamento:

- `invoice_number_sequence` gera a numeração sequencial;
- `invoices` possui índice único para o número;
- `invoice_items` possui chave estrangeira local para a nota e unicidade por nota e produto.

As migrations podem ser aplicadas na inicialização do ambiente de desenvolvimento e do Docker Compose. Em outros ambientes, essa opção é configurável.

## 13. Atomicidade, concorrência e idempotência

Embora fossem opcionais no enunciado original, concorrência e idempotência foram implementadas.

### 13.1. Atomicidade

A baixa de vários produtos é executada dentro de uma transação local do Estoque. Se qualquer produto não existir ou não tiver saldo suficiente, ocorre rollback e nenhuma baixa parcial permanece no banco.

### 13.2. Concorrência

Cada saldo é atualizado por uma instrução condicional equivalente a:

```sql
UPDATE products
SET balance = balance - @quantity
WHERE id = @productId
  AND balance >= @quantity;
```

Assim, duas requisições concorrentes sobre a última unidade não conseguem consumir o mesmo saldo: uma atualização é concluída e a outra recebe conflito por saldo insuficiente. O saldo final permanece igual a zero, nunca negativo.

### 13.3. Idempotência

O Faturamento reutiliza a chave `invoice:{invoiceId}:close:v1` em toda tentativa lógica de fechamento. O Estoque persiste a chave, o identificador da nota e o hash canônico do conteúdo.

- mesma chave e mesmo conteúdo: retorna o resultado já processado, sem nova baixa;
- mesma chave e conteúdo diferente: retorna conflito de idempotência;
- o registro da operação e as baixas são confirmados na mesma transação.

Essa proteção cobre clique duplicado, repetição manual e o caso em que o Estoque conclui a baixa, mas a resposta não chega ao Faturamento.

## 14. Falha e recuperação entre microsserviços

O cenário demonstrável utiliza a indisponibilidade do Estoque:

1. uma nota aberta é preparada;
2. o container `estoque-api` é interrompido;
3. a emissão é solicitada;
4. o Faturamento responde com indisponibilidade, a interface apresenta feedback em português e a nota permanece aberta;
5. o Estoque é iniciado novamente;
6. a emissão é repetida com a mesma chave idempotente;
7. o estoque é baixado uma única vez e a nota é fechada.

Não existe transação distribuída. A consistência é obtida por transações locais, confirmação explícita e idempotência.

## 15. APIs, segurança operacional e observabilidade

As APIs possuem:

- rotas versionadas sob `/api/v1`;
- especificação em `/openapi/v1.json`;
- documentação navegável com Scalar em `/docs`;
- CORS configurável para as origens do frontend;
- rate limiting local com resposta `429`;
- logging estruturado e `traceId`;
- configuração de URL e timeout do Estoque;
- health checks utilizados pelo Docker Compose.

Autenticação e autorização não foram implementadas porque o desafio não define usuários ou perfis de acesso.

## 16. Testes automatizados

O backend possui testes unitários com xUnit e Shouldly para as invariantes e os casos de uso. Os testes de integração utilizam SQL Server real iniciado por Testcontainers.

Entre os cenários cobertos estão:

- saldo inicial negativo;
- baixa inválida e saldo insuficiente;
- baixa válida;
- nota iniciando aberta;
- item inválido ou repetido;
- fechamento de nota aberta e rejeição de novo fechamento;
- persistência e restrições reais do SQL Server;
- código de produto único;
- numeração sequencial da nota;
- baixa atômica e rollback;
- idempotência com o mesmo conteúdo;
- conflito com conteúdo diferente;
- concorrência pela última unidade;
- falha do Estoque mantendo a nota aberta;
- retentativa que fecha a nota sem segunda baixa;
- geração do PDF.

O frontend possui testes com Vitest para componentes, validações, serviços HTTP, feedback, emissão e visualização do documento.

## 17. Execução do projeto

Na raiz do repositório:

```powershell
docker compose -f docker-compose.full.yml up -d --build
docker compose -f docker-compose.full.yml ps
```

Endereços do ambiente completo:

| Componente | Endereço |
|---|---|
| Aplicação Angular | `http://localhost:4201` |
| API de Estoque | `http://localhost:5237` |
| Documentação do Estoque | `http://localhost:5237/docs` |
| API de Faturamento | `http://localhost:5093` |
| Documentação do Faturamento | `http://localhost:5093/docs` |

Para demonstrar a falha e a recuperação:

```powershell
docker compose -f docker-compose.full.yml stop estoque-api
docker compose -f docker-compose.full.yml start estoque-api
docker compose -f docker-compose.full.yml ps
```

As instruções completas de execução, migrations e testes estão no `README.md` da raiz do repositório.

## 18. Decisões de escopo e limitações

A solução representa uma nota simplificada de saída de produtos. Não é uma NF-e legal e não possui integração com SEFAZ, XML fiscal ou DANFE oficial.

Também ficaram fora do escopo:

- autenticação, usuários e autorização;
- edição e exclusão de produtos;
- ajuste manual ou entrada de estoque;
- edição, cancelamento, estorno ou reabertura de notas;
- clientes, preços, impostos, descontos e pagamentos;
- mensageria e API Gateway dedicado;
- inteligência artificial;
- infraestrutura de produção, como Kubernetes.

Essas limitações mantêm a implementação concentrada nos requisitos do desafio e nas regras críticas de integridade, falha e recuperação.
