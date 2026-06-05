# AnalictY Platform

O AnalictY passa a ser tratado como uma familia de produtos, cada um com uma responsabilidade clara.

```text
AnalictY Platform
├── AnalictY Server
├── AnalictY Web
└── AnalictY Manager
```

## AnalictY Server

O Server e a base instalada no Windows. Ele fica em segundo plano e executa o trabalho pesado da plataforma.

Responsabilidades:

- API local.
- Runtime e servicos Windows.
- Banco de dados.
- Integracoes industriais.
- Coleta e processamento.
- Logs tecnicos.
- Atualizacoes.
- Diagnosticos de ambiente.

O usuario operacional nao deve precisar interagir diretamente com o Server.

## AnalictY Web

O Web e a interface operacional da fabrica. Ele deve continuar bonito, simples e acessivel pelo navegador.

Responsabilidades:

- Visao geral.
- Status operacional.
- Historico de producao.
- Historico de paradas.
- Relatorios operacionais.
- Alertas.
- Dashboards.
- Configuracoes operacionais simples.

O Web nao deve concentrar funcoes tecnicas de manutencao do sistema instalado.

## AnalictY Manager

O Manager e o aplicativo Windows para administracao local do ambiente AnalictY.

Responsabilidades:

- Verificar saude do Server.
- Abrir o Web.
- Consultar logs.
- Diagnosticar ambiente local.
- Verificar e aplicar atualizacoes.
- Gerenciar servicos quando permitido.
- Apoiar suporte, manutencao e instalacao.
- Centralizar configuracoes tecnicas que nao devem ficar no Web.

O Manager nao substitui o Web. Ele e o escritorio do gerente e do tecnico.

## Regra De Produto

- Operacao usa AnalictY Web.
- Runtime e integracoes ficam no AnalictY Server.
- Administracao local e manutencao ficam no AnalictY Manager.

