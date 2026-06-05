# Project Brief: AnalictY Manager

## Contexto

O AnalictY atual e uma plataforma industrial instalavel para Windows, composta por servicos locais, backend, frontend web, banco local, Agent Tray e instalador.

Este repositorio deve criar o `AnalictY Manager`, aplicativo desktop nativo para administracao local do AnalictY Server.

## Objetivo

Criar um cliente Windows nativo para operar o AnalictY Server sem abrir navegador.

O cliente deve consumir a API local existente:

```text
http://127.0.0.1:5000
```

## Regras Obrigatorias

- Nao usar WebView2.
- Nao usar Electron.
- Nao embutir navegador.
- Nao copiar o frontend web.
- Nao alterar backend, frontend, instalador ou servicos do repositorio `scada_mes`.
- Manter o projeto compilavel de forma isolada.
- Usar WPF .NET 8.
- Usar MVVM simples.
- Nao persistir senha do usuario.
- Guardar token somente em memoria no escopo inicial.
- Falhas de API devem exibir estado vazio ou mensagem amigavel, nunca quebrar a janela.

## Estrutura Recomendada

```text
desktop-AnalictY.Manager
├── src
│   └── AnalictY.Manager
│       ├── App.xaml
│       ├── MainWindow.xaml
│       ├── Views
│       ├── ViewModels
│       ├── Models
│       ├── Services
│       └── Infrastructure
├── docs
└── README.md
```

## Telas do Escopo Inicial

### Splash / Server Status

- Consultar `GET /api/system/health`.
- Mostrar servidor online/offline.
- Botao `Tentar novamente`.
- Botao `Abrir logs`.

### Login

- Campo usuario.
- Campo senha.
- Chamar endpoint de login existente.
- Guardar token em memoria.
- Exibir erro amigavel quando credenciais falharem.

### Layout Principal

Sidebar:

- Visao Geral
- Status
- Historico Producao
- Historico Paradas
- Relatorio
- Alertas
- Configuracoes

Topbar:

- Nome `AnalictY Manager`.
- Status do AnalictY Server.
- Usuario logado.

### Visao Geral

- Cards simples:
  - Total maquinas.
  - Em operacao.
  - Manutencao.
  - Ociosas.

Se algum endpoint ainda nao existir ou falhar, mostrar estado vazio.

## Servicos Internos

- `ApiClient`
- `AuthService`
- `HealthService`
- `RuntimeStatusService`
- `NavigationService`

## Aparencia

- Visual industrial profissional.
- Sidebar escura.
- Conteudo claro.
- Tipografia limpa.
- Sem temas exagerados.

## Entrega Esperada

- Projeto WPF criado.
- Build funcionando.
- README atualizado com como rodar.
- Codigo simples, sem overengineering.
