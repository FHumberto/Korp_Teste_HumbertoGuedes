# Frontend Angular

Interface do sistema Korp para cadastro de produtos e emissão simplificada de notas fiscais.

## Versões suportadas

- Node.js `24.19.0` (registrado em `.nvmrc`);
- npm `11.17.0`;
- Angular `22.1.3`;
- Angular CLI e build `22.1.5`.

As dependências diretas usam versões exatas. O `package-lock.json` é parte obrigatória do repositório e determina toda a árvore transitiva.

## Instalação reproduzível

Execute na pasta `src/Web`:

```powershell
npm ci
```

Use `npm ci`, e não `npm install`, em ambientes de desenvolvimento limpos, CI e preparação da entrega. O comando falha quando `package.json` e `package-lock.json` divergem, evitando instalações não reproduzíveis.

## Executar

```powershell
npm start
```

A aplicação estará disponível em `http://localhost:4200/`.

Para executar o ambiente completo em containers, use a raiz do repositório:

```powershell
docker compose up -d --build
```

Nesse modo, o Angular é compilado em uma imagem Node multi-stage e servido pelo Nginx, que também encaminha as chamadas às duas APIs pela rede interna do Compose.

## Validar

```powershell
npm test -- --watch=false
npm run build
```

## Atualização de dependências

Não execute `npm update` como parte do fluxo normal. Atualizações devem ser feitas deliberadamente em mudança isolada:

1. escolha e registre as novas versões exatas no `package.json`;
2. regenere o `package-lock.json`;
3. execute `npm ci` em uma instalação limpa;
4. execute testes e build;
5. versione juntos `package.json` e `package-lock.json`.
