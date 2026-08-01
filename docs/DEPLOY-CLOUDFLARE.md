# Deploy Cloudflare gratuito — ChamadosCamarj

## Backend: Cloudflare Tunnel (grátis)

### Setup único (primeira vez)
```powershell
# 1. Autenticar no Cloudflare
& "C:\Program Files (x86)\cloudflared\cloudflared.exe" tunnel login

# 2. Criar túnel permanente
& "C:\Program Files (x86)\cloudflared\cloudflared.exe" tunnel create camarj-api

# 3. Criar config em %USERPROFILE%\.cloudflared\config.yml:
#    url: http://localhost:5000
#    tunnel: camarj-api
#    credentials-file: %USERPROFILE%\.cloudflared\<TUNNEL-UUID>.json

# 4. Criar DNS no Cloudflare Dashboard:
#    CNAME api.camarj.com.br → <TUNNEL-UUID>.cfargotunnel.com

# 5. Rodar
& "C:\Program Files (x86)\cloudflared\cloudflared.exe" tunnel run camarj-api
```

### Modo rápido (URL temporária, sem DNS)
```powershell
& "C:\Program Files (x86)\cloudflared\cloudflared.exe" tunnel --url http://localhost:5000
# Anote a URL tipo https://alguma-coisa.trycloudflare.com
```

---

## Frontend: Cloudflare Pages (grátis, CDN global)

1. Acesse https://pages.cloudflare.com → Create a project
2. Conecte o GitHub: `JVictorPacheco/ChamadosCamarj`
3. Configure:
   - **Build command:** `cd frontend && npm install && npm run build`
   - **Output directory:** `frontend/dist`
   - **Environment variable:** `VITE_API_BASE_URL` = URL do backend (tunnel)
4. Deploy automático a cada push no `develop`

---

## Portas locais
- Backend: `http://localhost:5000`
- Frontend dev: `http://localhost:5173`
- Scalar (API docs): `http://localhost:5000/scalar`
