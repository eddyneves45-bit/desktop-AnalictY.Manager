# AnalictY Console

Cliente desktop nativo Windows para o AnalictY Server.

Este projeto existe para criar uma interface instalada, com cara de software Windows, sem WebView2, sem Electron e sem navegador embutido.

## Objetivo

Criar um aplicativo desktop em .NET 8 WPF que consome a API local do AnalictY Server:

```text
http://127.0.0.1:5000
```

O AnalictY Server atual continua existindo e funcionando normalmente:

- Backend como servico Windows.
- Frontend web atual preservado.
- Banco local preservado.
- Agent/Tray preservado.
- Atualizacoes preservadas.

## Regra Principal

Este projeto nao deve quebrar nem substituir o AnalictY existente. Ele e um novo cliente desktop nativo sobre a API existente.

## Escopo Inicial

- Splash/status do servidor.
- Login.
- Layout principal com sidebar.
- Visao Geral.
- Status.
- Historico Producao.
- Historico Paradas.
- Relatorio.
- Alertas.
- Configuracoes.

## Tecnologia

- .NET 8.
- WPF.
- MVVM simples.
- HttpClient.
- Sem WebView2.
- Sem Electron.

## Build e execucao

Requisitos:

- Windows.
- .NET SDK 8 instalado.
- Para executar a pasta publicada em outro computador: .NET Desktop Runtime 8 instalado.

Restaurar e compilar:

```powershell
dotnet build AnalictY.Console.sln
```

Executar o cliente desktop:

```powershell
dotnet run --project src/AnalictY.Console/AnalictY.Console.csproj
```

## Publicacao Desktop

Para gerar uma versao testavel em pasta local:

```powershell
.\scripts\publish-console.ps1
```

O executavel publicado fica em:

```text
release\desktop-console\AnalictY.Console.exe
```

A publicacao atual e dependente do runtime do .NET para manter o pacote pequeno. Instalador completo e pacote auto-contido ficam para uma etapa separada.

O executavel usa o icone oficial em:

```text
assets\analicty.ico
```

O botao `Verificar servidor` consulta:

```text
GET http://127.0.0.1:5000/api/system/health
```
