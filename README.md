# AnalictY Manager

Aplicativo desktop nativo Windows para administracao local do AnalictY Server.

Este projeto e o **AnalictY Manager**, o escritorio do gerente/tecnico dentro da familia AnalictY Platform.

Ele nao substitui o AnalictY Web. O Web continua sendo a interface operacional da fabrica.

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

## Familia De Produto

```text
AnalictY Platform
├── AnalictY Server
├── AnalictY Web
└── AnalictY Manager
```

Veja tambem: [PRODUCT_PORTFOLIO.md](PRODUCT_PORTFOLIO.md).

## Regra Principal

Este projeto nao deve quebrar nem substituir o AnalictY existente. Ele e um aplicativo desktop nativo de administracao sobre a API existente.

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
dotnet build AnalictY.Manager.sln
```

Executar o Manager:

```powershell
dotnet run --project src/AnalictY.Manager/AnalictY.Manager.csproj
```

## Publicacao Desktop

Para gerar uma versao testavel em pasta local:

```powershell
.\scripts\publish-manager.ps1
```

O executavel publicado fica em:

```text
release\desktop-manager\AnalictY.Manager.exe
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
