# Integrações Externas

## Implementadas

### PostgreSQL / Supabase (dev e produção)
- **Tipo:** Banco relacional gerenciado
- **Pacote:** `Npgsql.EntityFrameworkCore.PostgreSQL`
- **Config:** `appsettings.json` (host/usuário, sem senha) + `dotnet user-secrets` (senha, dev) / variável de ambiente (prod)
- **Conexão:** Session pooler (`aws-1-us-east-2.pooler.supabase.com:5432`) — não usar "Direct connection" (IPv6-only) nem "Transaction pooler" (incompatível com prepared statements do EF Core)
- **Uso:** `MigrateAsync()` na inicialização — aplica migrations reais, sem `EnsureCreated()`

---

## Planejadas (não implementadas)

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
