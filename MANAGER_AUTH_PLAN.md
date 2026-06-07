# MANAGER Authentication Plan

**Data:** 07/06/2026  
**Objetivo:** Planejar autenticação segura do AnalictY.Manager para operações administrativas de escrita

---

## Modelo Atual de Autenticação do AnalictY.Server

### Mecanismos de Autenticação

O AnalictY.Server utiliza **JWT Bearer Authentication** com **Cookies** e **CSRF Protection**:

1. **JWT Token:**
   - Validade: 15 minutos
   - Enviado via cookie `access_token` ou header `Authorization: Bearer <token>`
   - Configurado em `Program.cs` com `JwtBearerDefaults.AuthenticationScheme`

2. **Cookies:**
   - `access_token` - JWT token (HttpOnly, 15 min)
   - `refresh_token` - Token para refresh (HttpOnly, 15 min)
   - `session_id` - ID da sessão (HttpOnly, 15 min)
   - `csrf_token` - Token CSRF (não HttpOnly, 15 min)

3. **CSRF Protection:**
   - Validado em todas as requisições POST/PUT/DELETE
   - Esperado no header `X-CSRF-Token`
   - Cookie `csrf_token` é gerado no login/refresh

### Endpoints de Autenticação

| Endpoint | Método | Descrição | Anônimo |
|----------|--------|-----------|---------|
| `/api/auth/login` | POST | Login (username, password) | ✅ Sim |
| `/api/auth/register` | POST | Registro (username, email, password) | ✅ Sim |
| `/api/auth/refresh` | POST | Refresh token | ✅ Sim |
| `/api/auth/logout` | POST | Logout | ❌ Não |
| `/api/auth/me` | GET | Usuário atual | ❌ Não |
| `/api/auth/mfa/status` | GET | Status MFA | ❌ Não |
| `/api/auth/mfa/setup` | POST | Configurar MFA | ❌ Não |
| `/api/auth/mfa/enable` | POST | Ativar MFA | ❌ Não |
| `/api/auth/mfa/disable` | POST | Desativar MFA | ❌ Não |

### Middleware de Autenticação

O `Program.cs` possui múltiplos middlewares de autenticação:

1. **JWT Bearer:** Valida o token JWT
2. **Session Cookie:** Valida o `session_id` no banco de dados
3. **CSRF Token:** Valida o header `X-CSRF-Token` contra o cookie
4. **Authorization:** Valida roles e permissões

---

## Lacunas no AnalictY.Manager

### 1. Autenticação Não Implementada

**Problema:** O Manager não autentica usuários antes de fazer requisições POST.

**Impacto:** Operações de escrita (criar backup, restaurar backup) falham com 400 Bad Request devido ao CSRF token.

**Causa:** 
- O `AuthService` existe mas não é utilizado pelos ViewModels
- Os serviços (BackupService, etc) usam HttpClient sem cookies
- Não há tela de login no Manager

### 2. CookieContainer Não Configurado

**Problema:** O `CookieContainer` no `AuthService` não é compartilhado com outros serviços.

**Impacto:** Cookies de login não são enviados nas requisições de outros serviços.

**Causa:** Cada serviço cria seu próprio `HttpClient` sem o `CookieContainer` compartilhado.

### 3. CSRF Token Não Enviado

**Problema:** Requisições POST não enviam o header `X-CSRF-Token`.

**Impacto:** Middleware de CSRF rejeita a requisição com 400 Bad Request.

**Causa:** Os serviços não leem o cookie `csrf_token` e não o enviam no header.

---

## Fluxo Recomendado para o Manager

### 1. Login

```
Usuário → Tela de Login → AuthService.LoginAsync()
                                    ↓
                            POST /api/auth/login
                                    ↓
                            Server valida credenciais
                                    ↓
                            Server retorna cookies:
                            - access_token
                            - refresh_token
                            - session_id
                            - csrf_token
                                    ↓
                            AuthService armazena cookies
                            em CookieContainer
                                    ↓
                            AuthService.CurrentSession = AuthSession
                                    ↓
                            MainWindow atualiza UI
```

### 2. Requisições Autenticadas

```
ViewModel → Service → HttpClient (com CookieContainer)
                                    ↓
                            Cookies são enviados automaticamente
                            (access_token, session_id, csrf_token)
                                    ↓
                            Server valida JWT e session
                                    ↓
                            Server valida CSRF token
                                    ↓
                            Server processa requisição
```

### 3. Logout

```
Usuário → Logout → AuthService.LogoutAsync()
                                    ↓
                            POST /api/auth/logout
                            + header X-CSRF-Token
                                    ↓
                            Server invalida sessão
                                    ↓
                            AuthService limpa cookies
                                    ↓
                            AuthService.CurrentSession = null
                                    ↓
                            MainWindow volta para tela de login
```

### 4. Refresh Token (Opcional)

```
AuthService detecta 401 Unauthorized
                                    ↓
                            POST /api/auth/refresh
                            + cookie refresh_token
                                    ↓
                            Server retorna novos cookies
                                    ↓
                            AuthService atualiza cookies
                                    ↓
                            Requisição original é reenviada
```

---

## Arquitetura Proposta

### 1. HttpClient Compartilhado

**Problema:** Cada serviço cria seu próprio `HttpClient`.

**Solução:** Usar um `HttpClient` compartilhado com `CookieContainer` em `AppServices`.

```csharp
// AppServices.cs
public static class AppServices
{
    private static readonly CookieContainer _cookieContainer = new();
    private static readonly HttpClient _httpClient = new(new HttpClientHandler
    {
        CookieContainer = _cookieContainer
    });

    public static HttpClient HttpClient => _httpClient;
    public static CookieContainer CookieContainer => _cookieContainer;
}
```

### 2. AuthService Utilizado por Todos os Serviços

**Problema:** Serviços não usam o `AuthService`.

**Solução:** Injetar `AuthService` nos ViewModels e verificar autenticação antes de operações de escrita.

```csharp
// BackupViewModel.cs
public async Task CreateBackupAsync()
{
    if (_authService.CurrentSession == null)
    {
        MessageBox.Show("Faça login para criar backup.");
        return;
    }

    var result = await _backupService.CreateBackupAsync();
    // ...
}
```

### 3. CSRF Token Automático

**Problema:** CSRF token não é enviado automaticamente.

**Solução:** Criar um `DelegatingHandler` para injetar o CSRF token automaticamente.

```csharp
// CsrfHandler.cs
public class CsrfHandler : DelegatingHandler
{
    private readonly CookieContainer _cookieContainer;

    public CsrfHandler(CookieContainer cookieContainer)
    {
        _cookieContainer = cookieContainer;
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

### 4. Tela de Login

**Problema:** Não há tela de login no Manager.

**Solução:** Criar `LoginView.xaml` e `LoginViewModel.cs`.

```csharp
// LoginViewModel.cs
public sealed class LoginViewModel : ObservableObject
{
    private readonly AuthService _authService;
    private string _username = string.Empty;
    private string _password = string.Empty;
    private bool _isLoading;

    public LoginViewModel(AuthService authService)
    {
        _authService = authService;
        LoginCommand = new RelayCommand(async _ => await LoginAsync());
    }

    public async Task LoginAsync()
    {
        IsLoading = true;
        var result = await _authService.LoginAsync(Username, Password);
        IsLoading = false;

        if (result.Success)
        {
            // Navegar para MainWindow
        }
        else
        {
            ErrorMessage = result.Message;
        }
    }
}
```

---

## Endpoints Necessários

### Existentes (Já Implementados)

| Endpoint | Método | Status |
|----------|--------|--------|
| `/api/auth/login` | POST | ✅ Disponível |
| `/api/auth/logout` | POST | ✅ Disponível |
| `/api/auth/refresh` | POST | ✅ Disponível |
| `/api/auth/me` | GET | ✅ Disponível |

### Não Necessários

Todos os endpoints de autenticação já existem no Server. Não é necessário criar novos endpoints.

---

## Riscos

### 1. Segurança de Sessão em Memória

**Risco:** Sessão armazenada apenas em memória é perdida ao fechar o aplicativo.

**Mitigação:** Aceitável para aplicação desktop. O usuário deve fazer login novamente ao abrir o aplicativo.

### 2. CookieContainer Compartilhado

**Risco:** Se o CookieContainer não for thread-safe, pode haver problemas de concorrência.

**Mitigação:** `CookieContainer` é thread-safe para leitura, mas não para escrita. Usar locks se necessário.

### 3. CSRF Token Expirado

**Risco:** CSRF token pode expirar antes do access token.

**Mitigação:** Implementar refresh automático do CSRF token ao receber 400 Bad Request.

### 4. MFA Não Implementado

**Risco:** Usuários com MFA habilitado não conseguirão fazer login.

**Mitigação:** O `AuthService` já detecta MFA e retorna erro. Implementar MFA em fase futura.

---

## Próxima Etapa Executável

### Fase 1: Configurar HttpClient Compartilhado

1. Atualizar `AppServices.cs` para usar `CookieContainer` compartilhado
2. Atualizar `MainWindow.xaml.cs` para usar o `HttpClient` compartilhado
3. Testar se cookies são persistidos entre requisições

### Fase 2: Implementar Tela de Login

1. Criar `LoginView.xaml`
2. Criar `LoginViewModel.cs`
3. Integrar com `AuthService`
4. Adicionar navegação entre LoginView e MainWindow

### Fase 3: Implementar CSRF Handler

1. Criar `CsrfHandler.cs`
2. Configurar `HttpClient` para usar o handler
3. Testar se CSRF token é enviado automaticamente

### Fase 4: Atualizar ViewModels para Verificar Autenticação

1. Atualizar `BackupViewModel` para verificar autenticação antes de criar/restore
2. Atualizar outros ViewModels que fazem operações de escrita
3. Testar operações de escrita com autenticação

### Fase 5: Implementar Refresh Token (Opcional)

1. Implementar refresh automático ao receber 401
2. Testar refresh de token
3. Testar reenvio de requisição original

---

## Conclusão

O AnalictY.Server possui um modelo de autenticação robusto com JWT, cookies e CSRF protection. O AnalictY.Manager já possui um `AuthService` funcional, mas não está sendo utilizado pelos ViewModels e serviços.

Para implementar autenticação completa no Manager, é necessário:

1. Configurar `HttpClient` compartilhado com `CookieContainer`
2. Implementar tela de login
3. Implementar `CsrfHandler` para envio automático do CSRF token
4. Atualizar ViewModels para verificar autenticação antes de operações de escrita

Nenhuma mudança é necessária no AnalictY.Server. Todos os endpoints de autenticação já existem e funcionam corretamente.
