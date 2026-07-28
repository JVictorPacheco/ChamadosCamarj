# Deploy Backend no Azure (Gratuito)

Backend .NET 9 hospedado no **Azure App Service Free (F1)** — grátis, sem custo.

## Por que Azure?

- Free tier (F1): 1 GB RAM, 60 CPU-min/dia — suficiente pro sistema interno da CAMARJ
- .NET 9 suporte nativo (sem Docker, sem gambiarra)
- Domínio `*.azurewebsites.net` com HTTPS automático
- Já usa Microsoft (mesmo tenant do Workspace da CAMARJ)

## Setup (fazer UMA vez)

### 1. Criar Azure App Service

1. Acesse https://portal.azure.com com a conta Microsoft da CAMARJ
2. Crie um **App Service**:
   - Subscription: sua assinatura
   - Resource Group: `chamadoscamarj` (criar novo)
   - Name: `chamadoscamarj-api`
   - Publish: **Code**
   - Runtime stack: **.NET 9**
   - OS: **Linux**
   - Region: **East US** (mais próximo do Supabase)
   - Pricing Plan: **Free F1** (Shared infrastructure)
3. Clique **Review + create** → **Create**

### 2. Configurar variáveis de ambiente (secrets)

No App Service criado, vá em **Settings → Environment variables** e adicione:

| Name | Value |
|------|-------|
| `ConnectionStrings__DefaultConnection` | `Host=aws-1-us-east-2.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.oxiqutweuejvopofbkoy;Password=ggVV8nIsMrEvlfFM;SSL Mode=Require;Trust Server Certificate=true` |
| `Email__SmtpEmail` | `suporte@camarj.com.br` |
| `Email__SmtpSenha` | `hvxottbnefbxrgng` |
| `Supabase__Url` | `https://oxiqutweuejvopofbkoy.supabase.co` |
| `Supabase__ServiceRoleKey` | `eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Im94aXF1dHdldWVqdm9wb2Zia295Iiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImlhdCI6MTc4MTc5ODc5MSwiZXhwIjoyMDk3Mzc0NzkxfQ.1dw3C46cScHtJaRI2V2jP_b_jL3l-XpZOTIF09wu9_0` |
| `Auth__JwtSigningKey` | `Q4BPXuafVHbnbNjg9M5RmfK3x2HNC+w2a2+P25t8i8p/8Fro4CXqZfiYDYESUSSu` |
| `Auth__FrontendBaseUrl` | `https://chamadoscamarj.pages.dev` |
| `ASPNETCORE_ENVIRONMENT` | `Production` |

> **Importante:** o separador de seções é `__` (dois underscores) — `Auth:JwtSigningKey` vira `Auth__JwtSigningKey`.

### 3. Configurar GitHub Actions

1. No App Service, vá em **Deployment → Deployment Center**
2. Source: **GitHub**
3. Autorize e selecione o repo `JVictorPacheco/ChamadosCamarj`
4. Branch: `main`
5. Workflow: o Azure vai gerar um `.yml` automaticamente — **NÃO usar**. Vá em "Skip" ou cancele.
6. Em vez disso, vá em **Overview → Get publish profile** e baixe o arquivo `.PublishSettings`

Depois no GitHub:
1. Acesse https://github.com/JVictorPacheco/ChamadosCamarj/settings/secrets/actions
2. Clique **New repository secret**
3. Name: `AZURE_WEBAPP_PUBLISH_PROFILE`
4. Value: cole o conteúdo inteiro do arquivo `.PublishSettings` baixado
5. Clique **Add secret**

O workflow `.github/workflows/deploy-azure.yml` já está no repo — ele vai disparar automaticamente a cada push na `main`.

### 4. Atualizar CORS

O CORS em `Program.cs` já inclui `https://chamadoscamarj.pages.dev`. Se a URL do Azure for diferente (ex: `https://chamadoscamarj-api.azurewebsites.net`), atualizar o CORS com essa URL também.

### 5. Atualizar o frontend

No Cloudflare Pages, atualize `VITE_API_BASE_URL`:
```
https://chamadoscamarj-api.azurewebsites.net/api
```

E dispare um redeploy.

---

## Como funciona depois de configurado

```
git push origin main → GitHub Actions builda + testa + deploya → Azure atualiza
```

- **Frontend:** sempre em `https://chamadoscamarj.pages.dev` (Cloudflare Pages)
- **Backend:** sempre em `https://chamadoscamarj-api.azurewebsites.net` (Azure)
- **Banco:** sempre no Supabase (nuvem, não muda)

Nunca mais cai túnel, nunca mais precisa de URL efêmera.

---

## Custos

| Recurso | Custo |
|---------|-------|
| Azure App Service F1 | **Grátis** |
| Cloudflare Pages | **Grátis** |
| Supabase | **Grátis** (já incluso no plano) |
| GitHub Actions | **Grátis** (2.000 min/mês) |

**Total: R$ 0,00**
