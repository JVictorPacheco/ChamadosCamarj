# 🐳 GUIA DOCKER (opcional)

> ⚠️ **Este não é o fluxo de desenvolvimento padrão do projeto.** O `docker-compose.yml` sobe um **PostgreSQL local isolado**, mas dev e produção hoje apontam para o **mesmo banco Supabase** (ver `.specs/codebase/STACK.md` e `.specs/project/STATE.md`). Rodar via Docker significa trabalhar com dados que não são os dados reais do Supabase. Para o fluxo real, veja o `README.md` (rodar API com `dotnet run` + frontend com `npm run dev`, ambos contra o Supabase).
>
> Use este guia só se quiser um ambiente local isolado (ex: testar migrations sem afetar o banco real).

## ✅ Pré-requisitos

- Docker Desktop instalado e rodando

## 🚀 Como Rodar

```bash
docker-compose up
```

Sobe 3 serviços: `postgres` (local, efêmero), `backend` (.NET API, conectado ao postgres local do compose, **não** ao Supabase) e `frontend` (Vite dev server).

Aguarde ~2-3 minutos na primeira vez (build das imagens).

## 🌐 URLs de Acesso

| Serviço | URL |
|---------|-----|
| **Frontend** | http://localhost:3000 (mapeado do 5173 do container) |
| **Backend** | http://localhost:5000 |
| **Swagger/Scalar** | http://localhost:5000/scalar |

## 🔐 Login

Não existe login por email/senha no sistema — a autenticação é um **seletor de perfil mockado** (Admin/Atendente/Solicitante) salvo em `localStorage`. Perfis atuais: Victor (Admin), Fábio (Atendente). O login Google Workspace real ainda está pendente (Fase 6, T09/T15) — ver `.specs/project/STATE.md`.

## 🛑 Parar os Containers

```bash
docker-compose down
```

## 🐛 Troubleshooting

### "Docker não encontrado"
```bash
# Instale Docker Desktop em: https://www.docker.com/products/docker-desktop
```

### "Port 3000 already in use"
```bash
docker-compose down
docker system prune -a
docker-compose up
```

### "Build failed"
```bash
docker-compose build --no-cache
docker-compose up
```

### Ver logs em tempo real
```bash
docker-compose logs -f frontend
docker-compose logs -f backend
```
