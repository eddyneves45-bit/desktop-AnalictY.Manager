# Plano de Endpoints Faltantes - AnalictY.Manager

**Data:** 07/06/2026  
**Objetivo:** Definir endpoints mínimos necessários no AnalictY.Server para finalizar as telas pendentes do Manager

---

## Resumo

| Tela | Prioridade | Endpoints Faltantes | Risco de Segurança |
|------|-----------|---------------------|-------------------|
| Backup | Alta | 3 | Médio |
| Local Server | Alta | 4 | Alto |
| Eventos | Média | 1 | Baixo |
| MySQL Console | Alta | 1 | Alto |
| Security | Alta | 3 | Alto |

---

## 1. Backup

### Objetivo da Tela
Gerenciar backups do banco de dados SQLite do AnalictY.Server, incluindo listagem, criação manual e restauração.

### Dados que Precisa Exibir
- Lista de backups existentes (nome, data, tamanho, localização)
- Status do último backup
- Configurações de backup (frequência, destino, retenção)
- Progresso de backup/restauração

### Ações que Precisa Executar
- Listar backups disponíveis
- Criar backup manual
- Restaurar backup específico
- Configurar agendamento de backup
- Excluir backup antigo

### Endpoints Existentes que Podem Ser Reutilizados
- `GET /api/admin/database/status` - Status do banco de dados
- `GET /api/system/health` - Verificar saúde do sistema antes de backup

### Endpoints Faltantes

#### 1.1 GET /api/admin/backups
**Descrição:** Listar todos os backups disponíveis  
**Resposta:**
```json
{
  "backups": [
    {
      "id": "backup_20250607_020000",
      "name": "backup_20250607_020000.db",
      "created_at": "2025-06-07T02:00:00Z",
      "size_bytes": 42800000,
      "location": "/data/backups/",
      "status": "completed"
    }
  ]
}
```
**Prioridade:** Alta  
**Risco de Segurança:** Baixo (leitura apenas)

#### 1.2 POST /api/admin/backups
**Descrição:** Criar backup manual do banco de dados  
**Request Body:**
```json
{
  "description": "Backup manual antes de manutenção"
}
```
**Resposta:**
```json
{
  "backup_id": "backup_20250607_030000",
  "status": "in_progress",
  "message": "Backup iniciado"
}
```
**Prioridade:** Alta  
**Risco de Segurança:** Médio (requer autenticação admin)

#### 1.3 POST /api/admin/backups/{id}/restore
**Descrição:** Restaurar backup específico  
**Request Body:**
```json
{
  "confirm": true
}
```
**Resposta:**
```json
{
  "status": "success",
  "message": "Backup restaurado com sucesso"
}
```
**Prioridade:** Alta  
**Risco de Segurança:** Alto (ação destrutiva, requer confirmação explícita)

### Risco de Segurança
- **Médio:** Operações de backup e restauração podem afetar integridade dos dados
- **Recomendação:** Exigir autenticação de nível admin para todas as operações
- **Recomendação:** Restauração deve exigir confirmação explícita (double-check)

### Prioridade
**Alta** - Backup é crítico para recuperação de desastres

---

## 2. Local Server

### Objetivo da Tela
Configurar e monitorar o servidor local AnalictY.Server, incluindo configuração de rede, firewall e comandos do sistema.

### Dados que Precisa Exibir
- Status do servidor local (online/offline)
- Endereço IP e porta
- Configuração de rede
- Status do firewall
- Informações do sistema (CPU, memória, disco)
- Logs do serviço Windows

### Ações que Precisa Executar
- Verificar status do servidor
- Obter informações de rede
- Verificar status do firewall
- Reiniciar serviço Windows
- Visualizar logs do sistema
- Testar conectividade

### Endpoints Existentes que Podem Ser Reutilizados
- `GET /api/system/health` - Status geral do sistema
- `GET /api/system/version` - Versão do sistema
- `GET /api/admin/services` - Status dos serviços

### Endpoints Faltantes

#### 2.1 GET /api/admin/server/info
**Descrição:** Obter informações detalhadas do servidor local  
**Resposta:**
```json
{
  "hostname": "SERVER-01",
  "ip_address": "192.168.1.100",
  "port": 5000,
  "os_version": "Windows Server 2022",
  "cpu_usage_percent": 25,
  "memory_usage_mb": 2048,
  "disk_usage_gb": 50,
  "uptime_seconds": 86400
}
```
**Prioridade:** Alta  
**Risco de Segurança:** Baixo (leitura apenas)

#### 2.2 GET /api/admin/server/network
**Descrição:** Obter configuração de rede e status do firewall  
**Resposta:**
```json
{
  "interfaces": [
    {
      "name": "Ethernet",
      "ip_address": "192.168.1.100",
      "subnet_mask": "255.255.255.0",
      "gateway": "192.168.1.1"
    }
  ],
  "firewall_enabled": true,
  "firewall_rules": [
    {
      "port": 5000,
      "allowed": true,
      "description": "AnalictY API"
    }
  ]
}
```
**Prioridade:** Alta  
**Risco de Segurança:** Médio (exposição de configuração de rede)

#### 2.3 POST /api/admin/server/restart
**Descrição:** Reiniciar o serviço AnalictY.Server  
**Request Body:**
```json
{
  "confirm": true,
  "reason": "Atualização de configuração"
}
```
**Resposta:**
```json
{
  "status": "success",
  "message": "Serviço reiniciado com sucesso"
}
```
**Prioridade:** Alta  
**Risco de Segurança:** Alto (interrupção do serviço)

#### 2.4 GET /api/admin/server/logs
**Descrição:** Obter logs do serviço Windows  
**Query Params:** `?lines=100&level=error`  
**Resposta:**
```json
{
  "logs": [
    {
      "timestamp": "2025-06-07T03:00:00Z",
      "level": "info",
      "message": "Serviço iniciado"
    }
  ]
}
```
**Prioridade:** Média  
**Risco de Segurança:** Baixo (leitura apenas)

### Risco de Segurança
- **Alto:** Reiniciar serviço pode causar interrupção
- **Médio:** Exposição de configuração de rede pode ser sensível
- **Recomendação:** Exigir autenticação de nível admin
- **Recomendação:** Reinício deve exigir confirmação explícita
- **Recomendação:** Logs podem conter informações sensíveis, filtrar se necessário

### Prioridade
**Alta** - Configuração do servidor local é essencial para operação

---

## 3. Eventos

### Objetivo da Tela
Exibir eventos do sistema em tempo real, incluindo alertas, mudanças de estado e notificações operacionais.

### Dados que Precisa Exibir
- Lista de eventos recentes
- Tipo de evento (alerta, informação, erro)
- Timestamp do evento
- Mensagem do evento
- Origem do evento
- Status (reconhecido/não reconhecido)

### Ações que Precisa Executar
- Listar eventos recentes
- Filtrar por tipo/severidade
- Reconhecer evento
- Buscar eventos por período

### Endpoints Existentes que Podem Ser Reutilizados
- `GET /api/alerts?limit=20` - Alertas existentes
- `GET /api/downtimes` - Eventos de parada
- `GET /api/audit/logs` - Logs de auditoria

### Endpoints Faltantes

#### 3.1 GET /api/events
**Descrição:** Listar eventos do sistema consolidados  
**Query Params:** `?limit=50&level=error&from=2025-06-01&to=2025-06-07`  
**Resposta:**
```json
{
  "events": [
    {
      "id": "evt_123",
      "timestamp": "2025-06-07T03:00:00Z",
      "level": "error",
      "source": "opcua",
      "message": "Conexão perdida com servidor OPC UA",
      "acknowledged": false
    }
  ],
  "total": 150
}
```
**Prioridade:** Média  
**Risco de Segurança:** Baixo (leitura apenas)

### Risco de Segurança
- **Baixo:** Apenas leitura de eventos
- **Recomendação:** Filtrar informações sensíveis nos eventos
- **Recomendação:** Exigir autenticação básica

### Prioridade
**Média** - Eventos são úteis mas existem alternativas (alerts, audit logs)

---

## 4. MySQL Console

### Objetivo da Tela
Executar consultas SQL técnicas no banco de dados MySQL MES para diagnóstico e manutenção.

### Dados que Precisa Exibir
- Editor de SQL
- Resultados da consulta em tabela
- Mensagens de erro do banco
- Histórico de consultas recentes
- Status da conexão MySQL

### Ações que Precisa Executar
- Executar consulta SQL
- Listar tabelas do banco
- Exibir estrutura de tabela
- Cancelar consulta em execução

### Endpoints Existentes que Podem Ser Reutilizados
- `GET /api/config/mysql/all` - Conexões MySQL configuradas
- `POST /api/config/mysql/{id}/test` - Testar conexão MySQL

### Endpoints Faltantes

#### 4.1 POST /api/system/sql
**Descrição:** Executar consulta SQL no banco MySQL  
**Request Body:**
```json
{
  "connection_id": "mysql_1",
  "query": "SELECT * FROM production_events LIMIT 10",
  "timeout_seconds": 30
}
```
**Resposta:**
```json
{
  "success": true,
  "columns": ["id", "machine_id", "timestamp", "quantity"],
  "rows": [
    [1, "machine_1", "2025-06-07T03:00:00Z", 100]
  ],
  "row_count": 1,
  "execution_time_ms": 15
}
```
**Prioridade:** Alta  
**Risco de Segurança:** **Muito Alto** - Execução arbitrária de SQL

### Risco de Segurança
- **Muito Alto:** Execução arbitrária de SQL pode comprometer o banco
- **Recomendação:** Exigir autenticação de nível admin
- **Recomendação:** Limitar a comandos SELECT por padrão
- **Recomendação:** Bloquear comandos destrutivos (DROP, DELETE, TRUNCATE) sem confirmação explícita
- **Recomendação:** Implementar timeout para evitar consultas longas
- **Recomendação:** Logar todas as consultas executadas
- **Recomendação:** Considerar implementar whitelist de tabelas permitidas

### Prioridade
**Alta** - Ferramenta técnica essencial para diagnóstico

---

## 5. Security

### Objetivo da Tela
Configurar e monitorar configurações de segurança técnica do AnalictY.Server.

### Dados que Precisa Exibir
- Status da autenticação
- Configurações de JWT (expiração, chave)
- Políticas de senha
- Status de MFA (se implementado)
- Logs de tentativas de login
- Certificados SSL/TLS

### Ações que Precisa Executar
- Visualizar configurações de segurança
- Alterar política de senha
- Gerar novo token JWT
- Revogar sessões ativas
- Visualizar logs de segurança

### Endpoints Existentes que Podem Ser Reutilizados
- `POST /api/auth/login` - Login
- `POST /api/auth/logout` - Logout
- `GET /api/auth/me` - Informações do usuário atual
- `GET /api/users` - Lista de usuários

### Endpoints Faltantes

#### 5.1 GET /api/admin/security/settings
**Descrição:** Obter configurações de segurança  
**Resposta:**
```json
{
  "jwt_expiration_minutes": 15,
  "password_min_length": 8,
  "password_require_uppercase": true,
  "password_require_number": true,
  "mfa_enabled": false,
  "max_login_attempts": 5,
  "lockout_duration_minutes": 30
}
```
**Prioridade:** Alta  
**Risco de Segurança:** Médio (exposição de configurações)

#### 5.2 PUT /api/admin/security/settings
**Descrição:** Atualizar configurações de segurança  
**Request Body:**
```json
{
  "jwt_expiration_minutes": 30,
  "password_min_length": 10
}
```
**Resposta:**
```json
{
  "status": "success",
  "message": "Configurações atualizadas"
}
```
**Prioridade:** Alta  
**Risco de Segurança:** Alto (alteração de políticas de segurança)

#### 5.3 GET /api/admin/security/audit
**Descrição:** Obter logs de segurança (tentativas de login, etc)  
**Query Params:** `?limit=50&from=2025-06-01`  
**Resposta:**
```json
{
  "events": [
    {
      "timestamp": "2025-06-07T03:00:00Z",
      "event_type": "login_attempt",
      "username": "admin",
      "ip_address": "192.168.1.100",
      "success": true
    }
  ]
}
```
**Prioridade:** Alta  
**Risco de Segurança:** Baixo (leitura apenas)

### Risco de Segurança
- **Alto:** Alteração de configurações de segurança pode comprometer o sistema
- **Médio:** Exposição de configurações pode revelar informações sensíveis
- **Recomendação:** Exigir autenticação de nível admin
- **Recomendação:** Alterações de configuração devem exigir confirmação
- **Recomendação:** Logar todas as alterações de configuração
- **Recomendação:** Implementar rollback automático se configuração inválida

### Prioridade
**Alta** - Segurança é crítica para operação segura

---

## Prioridade de Implementação

### Fase 1 - Crítica (Imediato)
1. **Backup** - Essencial para recuperação de desastres
2. **MySQL Console** - Ferramenta técnica essencial (com restrições de segurança)

### Fase 2 - Alta (Curto Prazo)
3. **Security** - Configurações de segurança são críticas
4. **Local Server** - Monitoramento e configuração do servidor

### Fase 3 - Média (Médio Prazo)
5. **Eventos** - Útil mas existem alternativas

---

## Recomendações Gerais

### Segurança
1. Todos os endpoints devem exigir autenticação JWT
2. Operações destrutivas devem exigir confirmação explícita
3. Logar todas as operações administrativas
4. Implementar rate limiting para endpoints sensíveis
5. Validar e sanitizar todos os inputs

### Arquitetura
1. Manager apenas consome APIs do Server
2. Nenhuma lógica de negócio crítica no Manager
3. Server deve validar todas as operações
4. Implementar tratamento de erros consistente

### Testes
1. Testar endpoints com autenticação
2. Testar endpoints sem autenticação (deve falhar)
3. Testar operações destrutivas com e sem confirmação
4. Testar timeout em consultas SQL longas

---

## Riscos e Mitigações

| Risco | Probabilidade | Impacto | Mitigação |
|-------|--------------|--------|-----------|
| SQL Injection no MySQL Console | Média | Alto | Validar queries, limitar comandos, whitelist |
| Backup corrompido | Baixa | Alto | Verificar integridade após backup |
| Reinício não autorizado do servidor | Baixa | Alto | Exigir confirmação, logar operação |
| Exposição de configurações sensíveis | Média | Médio | Filtrar campos sensíveis, exigir auth |
| Alteração maliciosa de segurança | Baixa | Alto | Exigir confirmação, logar, rollback |

---

## Próximos Passos

1. Revisar este plano com a equipe de backend
2. Priorizar endpoints da Fase 1
3. Implementar endpoints no AnalictY.Server
4. Implementar consumo no AnalictY.Manager
5. Testar integração ponta a ponta
6. Documentar APIs
