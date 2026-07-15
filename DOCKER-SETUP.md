# 🐳 GUIA DOCKER — FASE 6

## ✅ Pré-requisitos

- ✅ Docker Desktop instalado no MacBook
- ✅ Docker rodando (abra Docker Desktop)

## 🚀 Como Rodar Tudo

**1 comando = Backend + Frontend rodando!**

```bash
cd /Users/joaopacheco/Desktop/C#-.net/ChamadosCamarj
docker-compose up
```

Aguarde ~2-3 minutos na primeira vez (build das imagens).

## 🌐 URLs de Acesso

Quando ver "ready in xxx ms", abra no navegador:

| Serviço | URL |
|---------|-----|
| **Frontend** | http://localhost:3000 |
| **Backend** | http://localhost:5000 |
| **Swagger** | http://localhost:5000/swagger |

## 🔐 Credenciais de Teste

| Email | Senha | Perfil |
|-------|-------|--------|
| joão@camarj.com.br | senha123 | Cliente |
| victor@camarj.com.br | senha123 | Atendente |
| fábio@camarj.com.br | senha123 | Gerente |

## 🧪 6 Testes

Leia: `.specs/FRONTEND-FASE-6-TESTES.md`

```
[T1] Reatribuição de Chamado
[T2] Alterar Prioridade
[T3] Histórico (Timeline)
[T4] Comentários Internos
[T5] Filtro por Perfil
[T6] Tempo Real (SignalR)
```

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

---

**Pronto! Agora é só rodar! 🚀**
