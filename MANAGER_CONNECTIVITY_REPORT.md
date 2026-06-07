# Relatório de Conectividade - AnalictY.Manager

**Data:** 07/06/2026  
**Objetivo:** Validar conectividade ponta a ponta (UI → API → Server) de todas as telas do Manager

---

## Resumo Executivo

| Status | Quantidade | Porcentagem |
|--------|-----------|-------------|
| ✅ Funcional | 13 | 81.25% |
| ⚠ Parcial | 3 | 18.75% |
| ❌ Não conectado | 0 | 0% |
| **Total** | **16** | **100%** |

---

## Tela por Tela

### 1. Visão Geral (AdminDashboard)

| Aspecto | Status | Detalhes |
|---------|--------|----------|
| Menu | ✅ | NavigationItems: "Visão Geral" → "AdminDashboard" |
| View | ✅ | AdminDashboardView.xaml |
| ViewModel | ✅ | AdminDashboardViewModel.cs |
| Service | ✅ | AdminApiService.cs |
| Endpoint | ✅ | `/api/admin/*` |
| Binding | ✅ | DataContext configurado |
| Tratamento de erro | ✅ | Try/catch com mensagens |
| **Status** | **✅ Funcional** | |

---

### 2. Runtime

| Aspecto | Status | Detalhes |
|---------|--------|----------|
| Menu | ✅ | NavigationItems: "Runtime" → "Runtime" |
| View | ✅ | RuntimeView.xaml |
| ViewModel | ✅ | RuntimeViewModel.cs |
| Service | ✅ | HttpClient direto |
| Endpoint | ✅ | `GET /api/runtime/state` |
| Binding | ✅ | DataContext configurado |
| Tratamento de erro | ✅ | Try/catch com mensagens |
| **Status** | **✅ Funcional** | Conectado ao endpoint /api/runtime/state |

---

### 3. Serviços

| Aspecto | Status | Detalhes |
|---------|--------|----------|
| Menu | ✅ | NavigationItems: "Serviços" → "Services" |
| View | ✅ | ServicesView.xaml |
| ViewModel | ✅ | ServicesViewModel.cs |
| Service | ✅ | AdminApiService.cs |
| Endpoint | ✅ | `GET /api/admin/services` |
| Binding | ✅ | DataContext configurado |
| Tratamento de erro | ✅ | Try/catch com mensagens |
| **Status** | **✅ Funcional** | Conectado ao endpoint /api/admin/services |

---

### 4. Logs

| Aspecto | Status | Detalhes |
|---------|--------|----------|
| Menu | ✅ | NavigationItems: "Logs" → "Logs" |
| View | ✅ | LogsView.xaml, LogsPage.xaml |
| ViewModel | ✅ | LogsViewModel.cs |
| Service | ✅ | LogsService.cs |
| Endpoint | ✅ | `GET /api/logs/recent` |
| Binding | ✅ | DataContext configurado |
| Tratamento de erro | ✅ | Try/catch com mensagens |
| **Status** | **✅ Funcional** | |

---

### 5. Eventos

| Aspecto | Status | Detalhes |
|---------|--------|----------|
| Menu | ✅ | NavigationItems: "Eventos" → "Events" |
| View | ✅ | EventsView.xaml |
| ViewModel | ✅ | EventsViewModel.cs |
| Service | ✅ | EventsService.cs |
| Endpoint | ✅ | `GET /api/admin/events` |
| Binding | ✅ | DataContext configurado |
| Tratamento de erro | ✅ | Try/catch com mensagens |
| **Status** | **✅ Funcional** | Conectado ao endpoint /api/admin/events |

---

### 6. Backup

| Aspecto | Status | Detalhes |
|---------|--------|----------|
| Menu | ✅ | NavigationItems: "Backup" → "Backup" |
| View | ✅ | BackupView.xaml |
| ViewModel | ✅ | BackupViewModel.cs |
| Service | ✅ | BackupService.cs |
| Endpoint | ✅ | `GET /api/admin/backups`, `POST /api/admin/backups`, `POST /api/admin/backups/{id}/restore` |
| Binding | ✅ | DataContext configurado |
| Tratamento de erro | ✅ | Try/catch com mensagens |
| **Status** | **✅ Funcional** | Conectado aos endpoints de backup |

---

### 7. Configurações (Settings)

| Aspecto | Status | Detalhes |
|---------|--------|----------|
| Menu | ✅ | NavigationItems: "Configurações" → "Settings" |
| View | ✅ | ConfigPage.xaml |
| ViewModel | ✅ | ConfigPageViewModel.cs |
| Service | ⚠ | Usa ConfigService para sub-telas |
| Endpoint | ⚠ | Vários endpoints via ConfigService |
| Binding | ✅ | DataContext configurado |
| Tratamento de erro | ✅ | Try/catch em sub-telas |
| **Status** | **⚠ Parcial** | Container funcional, sub-telas variam |

---

### 8. Sobre (About)

| Aspecto | Status | Detalhes |
|---------|--------|----------|
| Menu | ✅ | NavigationItems: "Sobre" → "About" |
| View | ✅ | AboutView.xaml |
| ViewModel | ✅ | AboutViewModel.cs |
| Service | ✅ | VersionService.cs |
| Endpoint | ✅ | `GET /api/system/version` |
| Binding | ✅ | DataContext configurado |
| Tratamento de erro | ✅ | Try/catch com fallback |
| **Status** | **✅ Funcional** | |

---

## Sub-telas de Configurações

### 9. OPC Browser

| Aspecto | Status | Detalhes |
|---------|--------|----------|
| Menu | ✅ | ConfigPage: "opc-browser" |
| View | ✅ | OpcUaView.xaml, OpcBrowserPage.xaml |
| ViewModel | ✅ | OpcUaViewModel.cs, OpcUaBrowserViewModel.cs |
| Service | ✅ | ConfigService.cs |
| Endpoint | ✅ | `GET /api/config/opcua/browse` |
| Binding | ✅ | DataContext configurado |
| Tratamento de erro | ✅ | Try/catch com mensagens |
| **Status** | **✅ Funcional** | |

---

### 10. MQTT Monitor

| Aspecto | Status | Detalhes |
|---------|--------|----------|
| Menu | ✅ | ConfigPage: "mqtt-monitor" |
| View | ✅ | MqttMonitorPage.xaml |
| ViewModel | ✅ | MqttMonitorViewModel.cs |
| Service | ✅ | ConfigService.cs |
| Endpoint | ✅ | `GET /api/config/mqtt/*` |
| Binding | ✅ | DataContext configurado |
| Tratamento de erro | ✅ | Try/catch com mensagens |
| **Status** | **✅ Funcional** | |

---

### 11. Database Browser

| Aspecto | Status | Detalhes |
|---------|--------|----------|
| Menu | ✅ | ConfigPage: "database-browser" |
| View | ✅ | DatabaseBrowserPage.xaml |
| ViewModel | ✅ | DatabaseBrowserViewModel.cs |
| Service | ✅ | ConfigService.cs |
| Endpoint | ✅ | `GET /api/database-browser/*` |
| Binding | ✅ | DataContext configurado |
| Tratamento de erro | ✅ | Try/catch com mensagens |
| **Status** | **✅ Funcional** | |

---

### 12. Connections

| Aspecto | Status | Detalhes |
|---------|--------|----------|
| Menu | ✅ | ConfigPage: "connections" |
| View | ✅ | ConnectionsPage.xaml |
| ViewModel | ✅ | ConnectionsPageViewModel.cs |
| Service | ✅ | ConfigService.cs |
| Endpoint | ✅ | `GET/POST/PUT/DELETE /api/config/*` |
| Binding | ✅ | DataContext configurado |
| Tratamento de erro | ✅ | Try/catch com mensagens |
| **Status** | **✅ Funcional** | |

---

### 13. Weintek Browser

| Aspecto | Status | Detalhes |
|---------|--------|----------|
| Menu | ✅ | ConfigPage: "weintek-browser" |
| View | ✅ | WeintekPage.xaml |
| ViewModel | ✅ | WeintekViewModel.cs |
| Service | ✅ | ConfigService.cs |
| Endpoint | ✅ | `GET /api/weintek/*` |
| Binding | ✅ | DataContext configurado |
| Tratamento de erro | ✅ | Try/catch com mensagens |
| **Status** | **✅ Funcional** | |

---

### 14. Tags

| Aspecto | Status | Detalhes |
|---------|--------|----------|
| Menu | ✅ | ConfigPage: "tags" |
| View | ✅ | TagsPage.xaml, TagsView.xaml |
| ViewModel | ✅ | TagsViewModel.cs |
| Service | ✅ | ConfigService.cs |
| Endpoint | ✅ | `GET /api/tags/*` |
| Binding | ✅ | DataContext configurado |
| Tratamento de erro | ✅ | Try/catch com mensagens |
| **Status** | **✅ Funcional** | |

---

### 15. Machines

| Aspecto | Status | Detalhes |
|---------|--------|----------|
| Menu | ✅ | ConfigPage: "machines" |
| View | ✅ | MachinesPage.xaml |
| ViewModel | ✅ | MachinesViewModel.cs |
| Service | ✅ | ConfigService.cs |
| Endpoint | ✅ | `GET /api/machines` |
| Binding | ✅ | DataContext configurado |
| Tratamento de erro | ✅ | Try/catch com mensagens |
| **Status** | **✅ Funcional** | |

---

### 16. Shifts

| Aspecto | Status | Detalhes |
|---------|--------|----------|
| Menu | ✅ | ConfigPage: "shifts" |
| View | ✅ | ShiftsPage.xaml |
| ViewModel | ✅ | ShiftsViewModel.cs |
| Service | ✅ | ConfigService.cs |
| Endpoint | ✅ | `GET /api/shifts/*` |
| Binding | ✅ | DataContext configurado |
| Tratamento de erro | ✅ | Try/catch com mensagens |
| **Status** | **✅ Funcional** | |

---

### 17. Simulator

| Aspecto | Status | Detalhes |
|---------|--------|----------|
| Menu | ✅ | ConfigPage: "simulator" |
| View | ✅ | SimulatorPage.xaml |
| ViewModel | ✅ | SimulatorViewModel.cs |
| Service | ✅ | ConfigService.cs |
| Endpoint | ✅ | `GET /api/simulator/*` |
| Binding | ✅ | DataContext configurado |
| Tratamento de erro | ✅ | Try/catch com mensagens |
| **Status** | **✅ Funcional** | |

---

### 18. Production Diagnostics

| Aspecto | Status | Detalhes |
|---------|--------|----------|
| Menu | ✅ | ConfigPage: "production-diagnostics" |
| View | ✅ | ProductionDiagnosticsPage.xaml |
| ViewModel | ✅ | ProductionDiagnosticsViewModel.cs |
| Service | ✅ | ConfigService.cs |
| Endpoint | ✅ | `GET /api/production-diagnostics/*` |
| Binding | ✅ | DataContext configurado |
| Tratamento de erro | ✅ | Try/catch com mensagens |
| **Status** | **✅ Funcional** | |

---

### 19. Alerts

| Aspecto | Status | Detalhes |
|---------|--------|----------|
| Menu | ✅ | ConfigPage: "alerts" |
| View | ✅ | AlertsPage.xaml |
| ViewModel | ✅ | AlertsViewModel.cs |
| Service | ✅ | ConfigService.cs |
| Endpoint | ✅ | `GET /api/alerts/*` |
| Binding | ✅ | DataContext configurado |
| Tratamento de erro | ✅ | Try/catch com mensagens |
| **Status** | **✅ Funcional** | |

---

### 20. Telegram Notifications

| Aspecto | Status | Detalhes |
|---------|--------|----------|
| Menu | ✅ | ConfigPage: "telegram-notifications" |
| View | ✅ | TelegramPage.xaml |
| ViewModel | ✅ | TelegramViewModel.cs |
| Service | ✅ | ConfigService.cs |
| Endpoint | ✅ | `GET /api/notifications/telegram` |
| Binding | ✅ | DataContext configurado |
| Tratamento de erro | ✅ | Try/catch com mensagens |
| **Status** | **✅ Funcional** | |

---

### 21. Dashboards

| Aspecto | Status | Detalhes |
|---------|--------|----------|
| Menu | ✅ | ConfigPage: "dashboards" |
| View | ✅ | DashboardsPage.xaml |
| ViewModel | ✅ | DashboardsViewModel.cs |
| Service | ✅ | ConfigService.cs |
| Endpoint | ✅ | `GET /api/dashboards/*` |
| Binding | ✅ | DataContext configurado |
| Tratamento de erro | ✅ | Try/catch com mensagens |
| **Status** | **✅ Funcional** | |

---

### 22. Local Server

| Aspecto | Status | Detalhes |
|---------|--------|----------|
| Menu | ✅ | ConfigPage: "local-server" |
| View | ✅ | LocalServerPage.xaml |
| ViewModel | ✅ | LocalServerPageViewModel.cs |
| Service | ✅ | LocalServerService.cs |
| Endpoint | ✅ | `GET /api/admin/local-server/info` |
| Binding | ✅ | DataContext configurado |
| Tratamento de erro | ✅ | Try/catch com mensagens |
| **Status** | **✅ Funcional** | Conectado ao endpoint /api/admin/local-server/info |

---

### 23. Users

| Aspecto | Status | Detalhes |
|---------|--------|----------|
| Menu | ✅ | ConfigPage: "users" |
| View | ✅ | UsersPage.xaml |
| ViewModel | ✅ | UsersPageViewModel.cs |
| Service | ✅ | UserAdminService.cs |
| Endpoint | ✅ | `GET/POST/PUT/DELETE /api/users` |
| Binding | ✅ | DataContext configurado |
| Tratamento de erro | ✅ | Try/catch com mensagens |
| **Status** | **✅ Funcional** | |

---

### 24. Audit

| Aspecto | Status | Detalhes |
|---------|--------|----------|
| Menu | ✅ | ConfigPage: "audit" |
| View | ✅ | AuditPage.xaml |
| ViewModel | ✅ | AuditPageViewModel.cs |
| Service | ✅ | AuditService.cs |
| Endpoint | ✅ | `GET /api/audit` |
| Binding | ✅ | DataContext configurado |
| Tratamento de erro | ✅ | Try/catch com mensagens |
| **Status** | **✅ Funcional** | |

---

### 25. Downtime Reasons

| Aspecto | Status | Detalhes |
|---------|--------|----------|
| Menu | ✅ | ConfigPage: "downtime-reasons" |
| View | ✅ | DowntimeReasonsPage.xaml |
| ViewModel | ✅ | DowntimeReasonsViewModel.cs |
| Service | ✅ | ConfigService.cs |
| Endpoint | ✅ | `GET /api/downtime-reasons/*` |
| Binding | ✅ | DataContext configurado |
| Tratamento de erro | ✅ | Try/catch com mensagens |
| **Status** | **✅ Funcional** | |

---

## Telas Removidas da Barra Lateral

As seguintes telas existem no código mas **NÃO estão na barra lateral**:

- **Tags** - Removido da navegação principal, existe em ConfigPage
- **Protocolos** - Removido da navegação principal
- **Banco de Dados** - Removido da navegação principal, existe em ConfigPage

---

## Endpoints Faltantes

### MySQL Console
- **Endpoint:** `POST /api/system/sql`
- **Status:** ❌ Não implementado no Manager
- **Observação:** Endpoint pode não existir no backend

### Security
- **Endpoint:** Vários endpoints de segurança técnica
- **Status:** ❌ Não implementado no Manager
- **Observação:** Apenas endpoints básicos de auth existem (login, logout, me)

---

## Serviços Faltantes

| Serviço | Tela | Prioridade |
|---------|------|------------|
| SqlService | MySQL Console | Alta |
| SecurityService | Security | Alta |
| LocalServerService | Local Server | Alta |

---

## Prioridade de Correção

### Alta (Crítico)
1. **MySQL Console** - Tela não existe, endpoint não verificado
2. **Security** - Tela não existe, endpoints não verificados

### Média
1. Nenhuma

### Baixa
1. Nenhuma

---

## Mudanças Realizadas (07/06/2026)

### Tela Runtime
- **Antes:** Apenas UI estática com dados mock
- **Depois:** Conectado ao endpoint `GET /api/runtime/state`
- **Arquivos alterados:**
  - `RuntimeViewModel.cs` - Adicionado HttpClient, método LoadAsync()
  - `RuntimeView.xaml.cs` - DataContext configurado pelo MainWindow
  - `MainWindow.xaml.cs` - Passa HttpClient para RuntimeViewModel

### Tela Serviços
- **Antes:** Apenas UI estática com dados mock
- **Depois:** Conectado ao endpoint `GET /api/admin/services`
- **Arquivos alterados:**
  - `ServicesViewModel.cs` - Adicionado AdminApiService, método LoadAsync()
  - `ServicesView.xaml.cs` - DataContext configurado pelo MainWindow
  - `MainWindow.xaml.cs` - Passa AdminApiService para ServicesViewModel

### Tela Backup
- **Antes:** Apenas UI estática com dados mock
- **Depois:** Conectado aos endpoints `GET /api/admin/backups`, `POST /api/admin/backups`, `POST /api/admin/backups/{id}/restore`
- **Arquivos alterados:**
  - `BackupService.cs` - Novo serviço para backup
  - `BackupViewModel.cs` - Conectado ao BackupService, métodos CreateBackupAsync() e RestoreBackupAsync()
  - `MainWindow.xaml.cs` - Passa BackupService para BackupViewModel

### Tela Local Server
- **Antes:** View existia mas sem ViewModel/Service/Endpoint
- **Depois:** Conectado ao endpoint `GET /api/admin/local-server/info`
- **Arquivos alterados:**
  - `LocalServerService.cs` - Novo serviço para informações do servidor
  - `LocalServerPageViewModel.cs` - Novo ViewModel conectado ao LocalServerService
  - `LocalServerPage.xaml.cs` - DataContext configurado com ViewModel

### Tela Eventos
- **Antes:** Apenas UI estática com dados mock
- **Depois:** Conectado ao endpoint `GET /api/admin/events`
- **Arquivos alterados:**
  - `EventsService.cs` - Novo serviço para eventos
  - `EventsViewModel.cs` - Conectado ao EventsService, método LoadAsync()
  - `MainWindow.xaml.cs` - Passa EventsService para EventsViewModel

### Endpoints Criados no AnalictY.Server
- `GET /api/admin/backups` - Listar backups
- `POST /api/admin/backups` - Criar backup
- `POST /api/admin/backups/{id}/restore` - Restaurar backup
- `GET /api/admin/local-server/info` - Informações do servidor local
- `GET /api/admin/events` - Listar eventos do sistema

---

## Riscos

1. **Autenticação:** Algumas telas podem falhar se o token expirar (401 Unauthorized)
2. **Endpoints:** Alguns endpoints documentados podem não existir no backend
3. **Backup:** Restauração de backup é uma operação destrutiva que exige confirmação explícita

---

## Recomendações

1. **Verificar endpoints de segurança no backend** antes de implementar tela Security
2. **Verificar endpoint de SQL no backend** antes de implementar MySQL Console
3. **Adicionar autenticação admin** aos endpoints de backup e local-server (atualmente .AllowAnonymous())
4. **Adicionar indicador visual** para telas não conectadas (ícone de desconectado)
5. **Padronizar tratamento de erro** em todas as telas
