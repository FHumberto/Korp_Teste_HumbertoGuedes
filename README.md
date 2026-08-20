# Korp — Desafio Técnico

Sistema de emissão de notas fiscais desenvolvido como parte do desafio técnico da **Korp**.

## Testes de integração

Os testes de integração usam Testcontainers para iniciar automaticamente um SQL Server descartável. A suíte cria bancos separados para Faturamento e Estoque, aplica as migrations e remove o container ao terminar.

```powershell
dotnet test src/Faturamento/Korp.Faturamento.IntegrationTests/Korp.Faturamento.IntegrationTests.csproj
```

Também é possível executar diretamente pelo Test Explorer do Visual Studio ou Rider. O único requisito é que o Docker Desktop esteja aberto; não é necessário iniciar o Docker Compose ou configurar portas manualmente.

Foram mantidos somente os cenários essenciais: persistência real, numeração sequencial, atomicidade da baixa, idempotência com retentativa e concorrência pela última unidade.
