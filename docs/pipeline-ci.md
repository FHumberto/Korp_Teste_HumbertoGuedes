# Pipeline de integração contínua

O workflow `CI`, localizado em `.github/workflows/ci.yml`, valida cada pull request direcionado à branch `main` e cada atualização feita nessa branch.

## Validações obrigatórias

- **Estoque:** restore, build, testes unitários e testes de integração com SQL Server via Testcontainers.
- **Faturamento:** restore, build, testes unitários e testes de integração com SQL Server via Testcontainers.
- **Frontend:** instalação reproduzível com `npm ci`, testes com Vitest e build de produção.
- **Containers:** build das imagens de Estoque, Faturamento e frontend.
- **CI / Gate:** consolida os resultados anteriores e só passa quando todos forem concluídos com sucesso.

Os jobs são executados em paralelo. Uma execução anterior do mesmo pull request é cancelada quando novos commits são enviados. Resultados e cobertura dos testes .NET ficam disponíveis como artefatos por 14 dias.

## Proteção da branch principal

A branch `main` utiliza uma regra de proteção com estas definições:

1. Exigir pull request antes do merge.
2. Invalidar aprovações quando novos commits forem enviados. A quantidade mínima está em zero porque existe apenas um colaborador; eleve para uma aprovação quando houver outro revisor.
3. Exigir que todas as conversas estejam resolvidas.
4. Exigir que a branch esteja atualizada antes do merge ou habilitar a merge queue.
5. Exigir **`CI / Gate`** como status check obrigatório.
6. Bloquear force push e exclusão da branch.
7. Não permitir bypass, exceto para um papel de emergência previamente definido.

Somente o check agregado `CI / Gate` é obrigatório na proteção. Os jobs internos continuam obrigatórios por serem dependências desse gate, mas podem ser reorganizados sem alterar a proteção da branch. A configuração aplicada também está registrada em `.github/branch-protection-main.json` para auditoria.

## Execução local equivalente

O ambiente local precisa ter .NET 10, Node.js 24.19, npm 11.17 e Docker em execução.

```powershell
dotnet test src/Estoque/Korp.Estoque.UnitTests/Korp.Estoque.UnitTests.csproj --configuration Release
dotnet test src/Estoque/Korp.Estoque.IntegrationTests/Korp.Estoque.IntegrationTests.csproj --configuration Release
dotnet test src/Faturamento/Korp.Faturamento.UnitTests/Korp.Faturamento.UnitTests.csproj --configuration Release
dotnet test src/Faturamento/Korp.Faturamento.IntegrationTests/Korp.Faturamento.IntegrationTests.csproj --configuration Release

Set-Location src/Web
npm ci
npm test -- --watch=false
npm run build
```

Os testes de integração iniciam e removem automaticamente seus containers de SQL Server.
