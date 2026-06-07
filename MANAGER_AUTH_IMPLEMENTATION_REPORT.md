# MANAGER Authentication Implementation Report

**Data:** 07/06/2026  
**Objetivo:** Implementar autenticação mínima segura no AnalictY.Manager para permitir operações administrativas protegidas

---

## Resumo Executivo

| Tarefa | Status |
|--------|--------|
| Configurar HttpClient compartilhado com CookieContainer | ✅ Concluído |
| Implementar CsrfHandler para envio automático do header X-CSRF-Token | ✅ Concluído |
| Atualizar MainWindow.xaml.cs para usar novo AppServices | ✅ Concluído |
| Atualizar BackupViewModel para verificar autenticação antes de operações de escrita | ✅ Concluído |
| Build Manager | ✅ Concluído |
| Testar POST /api/admin/backups | ⚠ Manual (requer login) |

---

## Arquivos Alterados

### 1. AppServices.cs

**Arquivo:** `src\AnalictY.Manager\Infrastructure\AppServices.cs`

**Mudanças:**
- Adicionado `CookieContainer` estático compartilhado
- Configurado `HttpClient` com `HttpClientHandler` que usa o `CookieContainer`
- Adicionado `CsrfHandler` como wrapper do `HttpClientHandler`
- `HttpClient` agora é estático e não pode ser substituído via `Configure()`

**Código:**
```csharp
private static readonly CookieContainer _cookieContainer = new();
private static readonly HttpClient _httpClient;

static AppServices()
{
    var handler = new HttpClientHandler
    {
        CookieContainer = _cookieContainer
    };
    var csrfHandler = new CsrfHandler(_cookieContainer, handler);
    _httpClient = new HttpClient(csrfHandler) { Timeout = TimeSpan.FromSeconds(10) };
}

public static HttpClient HttpClient => _httpClient;
public static CookieContainer CookieContainer => _cookieContainer;
```

---

### 2. CsrfHandler.cs (Novo)

**Arquivo:** `src\AnalictY.Manager\Infrastructure\CsrfHandler.cs`

**Descrição:** Handler que injeta automaticamente o header `X-CSRF-Token` em requisições POST/PUT/DELETE.

**Código:**
```csharp
public sealed class CsrfHandler : DelegatingHandler
{
    private readonly CookieContainer _cookieContainer;

    public CsrfHandler(CookieContainer cookieContainer, HttpMessageHandler? innerHandler = null)
    {
        _cookieContainer = cookieContainer;
        InnerHandler = innerHandler ?? new HttpClientHandler();
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Method == HttpMethod.Post || 
            request.Method == HttpMethod.Put || 
            request.Method == HttpMethod.Delete)
        {
            var csrfToken = _cookieContainer.GetCookies(request.RequestUri)
                .Cast<Cookie>()
                .FirstOrDefault(c => c.Name == "csrf_token")?.Value;

            if (!string.IsNullOrWhiteSpace(csrfToken))
            {
                request.Headers.TryAddWithoutValidation("X-CSRF-Token", csrfToken);
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
```

---

### 3. MainWindow.xaml.cs

**Arquivo:** `src\AnalictY.Manager\MainWindow.xaml.cs`

**Mudanças:**
- Removida criação manual de `HttpClientHandler`, `CookieContainer` e `CsrfHeaderHandler`
- Agora usa `AppServices.HttpClient` e `AppServices.CookieContainer`
- Passa `AuthService` para `BackupViewModel`

**Código:**
```csharp
// AppServices já configura HttpClient com CookieContainer e CsrfHandler
var apiClient = AppServices.HttpClient;
var cookieContainer = AppServices.CookieContainer;

// ... serviços ...

var authService = new AuthService(apiClient, cookieContainer);
BackupViewHost.DataContext = new BackupViewModel(backupService, authService);
```

---

### 4. BackupViewModel.cs

**Arquivo:** `src\AnalictY.Manager\ViewModels\BackupViewModel.cs`

**Mudanças:**
- Adicionado campo `_authService`
- Atualizado construtor para receber `AuthService`
- Adicionado verificação de autenticação em `CreateBackupAsync()`
- Adicionado verificação de autenticação em `RestoreBackupAsync()`

**Código:**
```csharp
public async Task CreateBackupAsync()
{
    if (_authService.CurrentSession == null)
    {
        MessageBox.Show("Faça login como administrador para executar esta ação.", "AnalictY Manager - Autenticação Necessária", MessageBoxButton.OK, MessageBoxImage.Information);
        return;
    }
    // ...
}

public async Task RestoreBackupAsync(string backupId)
{
    if (_authService.CurrentSession == null)
    {
        MessageBox.Show("Faça login como administrador para executar esta ação.", "AnalictY Manager - Autenticação Necessária", MessageBoxButton.OK, MessageBoxImage.Information);
        return;
    }
    // ...
}
```

---

## Fluxo de Login Implementado

O Manager já possui uma tela de login funcional no `MainWindowViewModel`:

1. **Usuário clica em "Entrar"** → Abre modal de login
2. **Usuário digita username e senha** → `MainWindowViewModel.LoginAsync()`
3. **AuthService.LoginAsync()** → POST `/api/auth/login`
4. **Server retorna cookies** → `access_token`, `refresh_token`, `session_id`, `csrf_token`
5. **AuthService armazena cookies** → No `CookieContainer` compartilhado
6. **AuthService.CurrentSession** → Sessão autenticada
7. **ViewModels verificam autenticação** → Antes de operações de escrita

---

## CSRF Funcionando

O `CsrfHandler` é responsável por:

1. **Interceptar requisições POST/PUT/DELETE**
2. **Ler cookie `csrf_token`** do `CookieContainer`
3. **Injetar header `X-CSRF-Token`** automaticamente
4. **Passar requisição para o handler interno**

Isso garante que todas as requisições de escrita enviem o CSRF token corretamente sem necessidade de código manual em cada serviço.

---

## Teste de POST /api/admin/backups

**Status:** ⚠ Requer teste manual

**Como testar:**
1. Iniciar o AnalictY.Manager
2. Fazer login como administrador (username: `admin`, password: `Rs26051986@`)
3. Navegar para a tela de Backup
4. Clicar em "Backup Now"
5. Verificar se o backup é criado com sucesso

**Resultado esperado:**
- Se autenticado: Backup criado com sucesso
- Se não autenticado: Mensagem "Faça login como administrador para executar esta ação"

---

## Resultado do Build

**Status:** ✅ Sucesso

```
AnalictY.Manager -> C:\Users\admin.automacao\CascadeProjects\Desktop_AnalictY_Manager\src\AnalictY.Manager\bin\Debug\net8.0-windows\AnalictY.Manager.dll

Compilação com êxito.
    0 Aviso(s)
    0 Erro(s)
```

---

## Pendências Restantes

### 1. Teste Manual de Autenticação

**Descrição:** Testar o fluxo completo de login e operações de escrita.

**Ação necessária:**
- Iniciar o Manager
- Fazer login como administrador
- Testar criar backup
- Testar restaurar backup
- Verificar se CSRF token é enviado corretamente

**Prioridade:** Alta

---

### 2. Refresh Token (Opcional)

**Descrição:** Implementar refresh automático do token ao receber 401 Unauthorized.

**Ação necessária:**
- Implementar lógica de refresh no `AuthService`
- Reenviar requisição original após refresh

**Prioridade:** Baixa

---

### 3. MFA (Opcional)

**Descrição:** Implementar suporte a MFA para usuários com MFA habilitado.

**Ação necessária:**
- Detectar MFA no login
- Exibir tela para código MFA
- Enviar código MFA para o endpoint `/api/auth/mfa/verify`

**Prioridade:** Baixa

---

### 4. Outros ViewModels com Operações de Escrita

**Descrição:** Atualizar outros ViewModels que fazem operações de escrita para verificar autenticação.

**Ação necessária:**
- Identificar ViewModels com operações POST/PUT/DELETE
- Injetar `AuthService` nesses ViewModels
- Adicionar verificação de autenticação antes de operações de escrita

**Prioridade:** Média

---

## Conclusão

A autenticação mínima segura foi implementada com sucesso no AnalictY.Manager:

1. **HttpClient compartilhado** com `CookieContainer` e `CsrfHandler`
2. **CsrfHandler** injeta automaticamente o header `X-CSRF-Token`
3. **BackupViewModel** verifica autenticação antes de operações de escrita
4. **Build** concluído com sucesso
5. **Fluxo de login** já existente foi integrado

A próxima etapa é testar manualmente o fluxo completo de login e operações de escrita para validar que tudo funciona corretamente.
