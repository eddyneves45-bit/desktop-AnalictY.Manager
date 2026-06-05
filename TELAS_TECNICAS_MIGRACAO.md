# Telas Técnicas - Migração do Web para Manager

## Telas Removidas do AnalictY.Web (devem ir para Manager)

### 1. Logs
- **Rota:** `/logs`
- **Arquivo:** `frontend/app/logs/page.tsx`
- **Funcionalidade:** Visualizador de logs técnicos do backend
- **Endpoint Backend:** `GET /api/logs/recent`
- **Prioridade:** Alta

### 2. Local Server
- **Rota:** `/local-server`
- **Arquivo:** `frontend/app/local-server/page.tsx`
- **Funcionalidade:** Configuração de rede local, comandos PowerShell, firewall
- **Endpoint Backend:** Vários endpoints de sistema
- **Prioridade:** Alta

### 3. Database Browser
- **Rota:** `/database-browser`
- **Arquivo:** `frontend/app/database-browser/page.tsx`
- **Funcionalidade:** Navegador visual de banco de dados SQLite
- **Endpoint Backend:** `GET /api/database-browser/*`
- **Prioridade:** Alta

### 4. MySQL Console
- **Rota:** `/mysql-console`
- **Arquivo:** `frontend/app/mysql-console/page.tsx`
- **Funcionalidade:** Console SQL para MySQL MES
- **Endpoint Backend:** `POST /api/system/sql`
- **Prioridade:** Alta

### 5. MQTT Monitor
- **Rota:** `/mqtt-monitor`
- **Arquivo:** `frontend/app/mqtt-monitor/page.tsx`
- **Funcionalidade:** Monitoramento técnico de conexões MQTT
- **Endpoint Backend:** `GET /api/config/mqtt`, SignalR events
- **Prioridade:** Média

### 6. OPC Browser
- **Rota:** `/opc-browser`
- **Arquivo:** `frontend/app/opc-browser/page.tsx`
- **Funcionalidade:** Navegador de nós OPC UA
- **Endpoint Backend:** `GET /api/config/opcua/browse`
- **Prioridade:** Média

### 7. Security
- **Rota:** `/security`
- **Arquivo:** `frontend/app/security/page.tsx`
- **Funcionalidade:** Configurações de segurança técnica
- **Endpoint Backend:** Vários endpoints de segurança
- **Prioridade:** Média

### 8. Connections
- **Rota:** `/connections`
- **Arquivo:** `frontend/app/connections/page.tsx`
- **Funcionalidade:** Configuração técnica de conexões industriais
- **Endpoint Backend:** `GET /api/config/*`
- **Prioridade:** Alta

### 9. Production Diagnostics
- **Rota:** `/production-diagnostics`
- **Arquivo:** `frontend/app/production-diagnostics/page.tsx`
- **Funcionalidade:** Diagnóstico técnico de produção
- **Endpoint Backend:** `GET /api/production-diagnostics/*`
- **Prioridade:** Média

### 10. Simulator
- **Rota:** `/simulator`
- **Arquivo:** `frontend/app/simulator/page.tsx`
- **Funcionalidade:** Simulador de máquinas virtuais
- **Endpoint Backend:** `GET /api/simulator/*`
- **Prioridade:** Baixa

### 11. Audit
- **Rota:** `/audit`
- **Arquivo:** `frontend/app/audit/page.tsx`
- **Funcionalidade:** Auditoria técnica de ações
- **Endpoint Backend:** `GET /api/audit`
- **Prioridade:** Alta

### 12. Users
- **Rota:** `/users`
- **Arquivo:** `frontend/app/users/page.tsx`
- **Funcionalidade:** Gerenciamento de usuários (administração)
- **Endpoint Backend:** `GET/POST/PUT/DELETE /api/users`
- **Prioridade:** Alta

### 13. Weintek Browser
- **Rota:** `/weintek-browser`
- **Arquivo:** `frontend/app/weintek-browser/page.tsx`
- **Funcionalidade:** Navegador/configuração Weintek
- **Endpoint Backend:** `GET /api/weintek/*`
- **Prioridade:** Baixa

### 14. Telegram Notifications
- **Rota:** `/telegram-notifications`
- **Arquivo:** `frontend/app/telegram-notifications/page.tsx`
- **Funcionalidade:** Configuração técnica de notificações Telegram
- **Endpoint Backend:** `GET /api/notifications/telegram`
- **Prioridade:** Média

## Implementação no Manager

Cada tela deve ser implementada como:
- **Tela nativa WPF** (não WebView)
- **MVVM pattern**
- **Consumo direto da API do Server**
- **UI moderna e consistente**

## Ordem de Implementação Sugerida

1. **Fase 1 - Críticas:**
   - Logs
   - Local Server/Status
   - Database Browser
   - MySQL Console

2. **Fase 2 - Importantes:**
   - Connections
   - Users
   - Audit
   - Security

3. **Fase 3 - Complementares:**
   - MQTT Monitor
   - OPC Browser
   - Production Diagnostics
   - Telegram Notifications
   - Simulator
   - Weintek Browser

## Observações

- As telas já existem no `scada_mes/frontend` como referência de funcionalidade
- A lógica de backend já está no Server (endpoints existem)
- O Manager precisa apenas consumir esses endpoints via HTTP
- Não é necessário recriar lógica de negócio, apenas a UI WPF
