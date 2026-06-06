# AnalictY Manager - Mapa de Migracao das Configuracoes

## Diagnostico

O AnalictY Web original ja possui as telas tecnicas funcionando e o AnalictY Server ja possui a maior parte dos endpoints necessarios.

O Manager hoje abre as paginas, mas muitas ainda sao telas WPF estaticas. O principal bloqueio para ligar tudo e autenticacao: as rotas tecnicas retornam `401` quando o Manager entra pelo atalho local `admin/admin`, porque esse atalho cria apenas uma sessao falsa no desktop e nao faz login no Server.

## Decisao de arquitetura

- AnalictY Server mantem regras, configuracoes, banco e endpoints.
- AnalictY Web fica focado em operacao industrial.
- AnalictY Manager vira a interface administrativa desktop do Server.
- Manager deve autenticar no Server de verdade para acessar configuracoes protegidas.
- O modo `admin/admin` pode continuar apenas como modo de desenvolvimento visual, mas nao deve ser usado para testar configuracoes reais.

## Endpoints ja existentes no Server

### Sistema

- `GET /api/system/health`
- `GET /api/system/version`
- `GET /api/system/updates/check`
- `GET /api/config/system/timezone`
- `PUT /api/config/system/timezone`
- `GET /api/config/system/time`
- `GET /api/config/system/local-server`
- `PUT /api/config/system/local-server`

### OPC UA

- `GET /api/config/opcua/all`
- `PUT /api/config/opcua`
- `DELETE /api/config/opcua/{id}`
- `GET /api/config/opcua/browse`
- `POST /api/drivers/opcua/connect`

### MQTT

- `GET /api/config/mqtt/all`
- `PUT /api/config/mqtt`
- `DELETE /api/config/mqtt/{id}`
- `POST /api/config/mqtt/{id}/test`
- `POST /api/config/mqtt/certificates/upload`
- `GET /api/config/mqtt/cache/topics`
- `GET /api/config/mqtt/clients`
- `POST /api/config/mqtt/publish`
- `POST /api/config/mqtt/subscribe`
- `POST /api/config/mqtt/unsubscribe`

### Banco de Dados

- `GET /api/config/mysql/all`
- `PUT /api/config/mysql`
- `DELETE /api/config/mysql/{id}`
- `POST /api/config/mysql/{id}/set-primary`
- `POST /api/config/mysql/{id}/set-local`
- `POST /api/config/mysql/{id}/set-remote`
- `POST /api/config/mysql/{id}/test`
- `POST /api/config/mysql/test`
- `POST /api/config/mysql/{id}/init`
- `GET /api/database-browser/connections`
- `GET /api/database-browser/connections/{id}/databases`
- `GET /api/database-browser/connections/{id}/tables`
- `GET /api/database-browser/connections/{id}/columns`
- `GET /api/database-browser/connections/{id}/rows`

### Tags e Producao

- `GET /api/config/tags`
- `POST /api/config/tags`
- `PUT /api/config/tags/{id}`
- `DELETE /api/config/tags/{id}`
- `GET /api/machine-folders`
- `GET /api/config/shifts`
- `PUT /api/config/shifts`
- `DELETE /api/config/shifts/{id}`
- `GET /api/machines/{id}/tag-mapping`
- `POST /api/machines/{id}/tag-mapping`
- `DELETE /api/machines/{id}/tag-mapping/{role}`
- `GET /api/machines/{id}/downtime-reasons`
- `PUT /api/machines/{id}/downtime-reasons`
- `DELETE /api/machines/{id}/downtime-reasons/{code}`

### Weintek HTTP

- `GET /api/config/weintek`
- `PUT /api/config/weintek`
- `GET /api/config/weintek/browser`
- `PUT /api/config/weintek/token`
- `DELETE /api/config/weintek/token`
- `POST /api/config/weintek/tags`
- `POST /api/weintek/ingest`
- `GET /api/weintek/ping`

### Telegram e Alertas

- `GET /api/notifications/telegram/status`
- `POST /api/notifications/telegram/test`
- `GET /api/notifications/telegram/connections`
- `POST /api/notifications/telegram/connections`
- `PUT /api/notifications/telegram/connections/{id}`
- `DELETE /api/notifications/telegram/connections/{id}`
- `GET /api/notifications/telegram/recipients`
- `GET /api/notifications/telegram/candidates`
- `POST /api/notifications/telegram/recipients`
- `PUT /api/notifications/telegram/recipients/{id}`
- `DELETE /api/notifications/telegram/recipients/{id}`
- `GET /api/alert-rules`
- `POST /api/alert-rules`
- `PUT /api/alert-rules/{id}`
- `DELETE /api/alert-rules/{id}`

### Administracao

- `GET /api/users`
- `POST /api/users`
- `PUT /api/users/{id}`
- `DELETE /api/users/{id}`
- `GET /api/audit/logs`
- `GET /api/logs/recent`

## Estado atual no Manager

### Ja ligado parcialmente

- Visao Geral Admin: usa `/api/admin/...`.
- Health/version: usa `/api/system/health` e `/api/system/version`.
- Alertas: ja consulta parte de alertas e Telegram, mas precisa sessao real.
- Producao, paradas e relatorios: usam endpoints operacionais existentes.

### Ainda estatico ou incompleto

- Runtime
- Tags
- Protocolos
- OPC UA
- MQTT
- Banco de Dados
- Servicos
- Logs
- Eventos
- Backup
- Configuracoes
- Usuarios
- Auditoria
- Weintek HTTP
- Turnos

## Ordem recomendada

1. Corrigir autenticacao do Manager.
   - O campo Servidor deve controlar a URL base.
   - O botao Conectar deve fazer login real em `/api/auth/login`.
   - O modo `admin/admin` deve ficar marcado como desenvolvimento visual, sem prometer acesso real.

2. Criar `ServerApiClient` central.
   - Base URL configuravel.
   - CookieContainer compartilhado.
   - GET/POST/PUT/DELETE com JSON.
   - Tratamento padrao de `401`, `403`, timeout e erro de conexao.

3. Migrar paginas de consulta primeiro.
   - Logs
   - Auditoria
   - Banco de Dados
   - Usuarios
   - MQTT status
   - OPC UA lista
   - Tags lista

4. Migrar paginas com escrita depois.
   - OPC UA criar/editar/excluir
   - MQTT criar/editar/excluir/testar
   - MySQL criar/editar/testar/primario
   - Telegram conexoes e destinatarios
   - Turnos
   - Tags
   - Weintek

5. So depois habilitar acoes perigosas.
   - Reiniciar servicos
   - Limpar logs
   - Backup agora
   - Otimizacao de banco
   - Exclusoes em massa

## Primeiro trabalho pratico

O proximo ajuste no codigo deve ser autenticar o Manager no Server real. Sem isso, toda migracao de configuracao fica falsa, porque a tela abre, mas os endpoints protegidos continuam retornando `401`.
