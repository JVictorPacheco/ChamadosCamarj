# Integrações Externas

## Implementadas

### SQLite (dev)
- **Tipo:** Banco local
- **Config:** `appsettings.json` → `ConnectionStrings:DefaultConnection`
- **Arquivo:** `src/ChamadosCamarj.WebApi/chamadoscamarj.db`
- **Uso:** `EnsureCreated()` — sem migrations em dev

---

## Planejadas (não implementadas)

### PostgreSQL / Supabase (produção)
- **Tipo:** Banco relacional gerenciado
- **Pacote:** `Npgsql.EntityFrameworkCore.PostgreSQL`
- **Config:** variável de ambiente / secrets em prod
- **Concern:** Migration atual usa tipos PostgreSQL mas o app roda SQLite (ver CONCERNS.md C-01)

### Azure AD (autenticação)
- **Tipo:** OAuth2 / OpenID Connect corporativo Microsoft
- **Interface definida:** não (sem `IAuthService`)
- **Impacto:** Fase 3 (Frontend) e Fase 6 (Admin)
- **Pacote esperado:** `Microsoft.Identity.Web`

### Supabase Storage (anexos)
- **Tipo:** Object storage S3-compatible
- **Interface definida:** `IStorageService` em `Domain/Interfaces/`
- **Implementação:** não existe ainda
- **Fase:** 4

### MailKit — IMAP (entrada de e-mails)
- **Tipo:** Captura de e-mails para criar chamados automaticamente
- **Interface definida:** `IEmailReceiverService` em `Domain/Interfaces/`
- **Implementação:** não existe ainda
- **Conta:** suporte@camarj.com.br / ti@camarj.com.br
- **Fase:** 4

### SignalR (tempo real)
- **Tipo:** WebSocket — notificações em tempo real no frontend
- **Fase:** 5
- **Uso previsto:** atualização de status do chamado sem refresh

### Serilog (logging)
- **Tipo:** Logging estruturado
- **Fase:** futura
