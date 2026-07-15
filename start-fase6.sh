#!/bin/bash
# Script para rodar Backend + Frontend simultaneamente
# Executar no MacBook: chmod +x start-fase6.sh && ./start-fase6.sh

set -e

echo "╔═════════════════════════════════════════════════════════════╗"
echo "║     🚀 INICIANDO FASE 6 — Backend + Frontend               ║"
echo "╚═════════════════════════════════════════════════════════════╝"
echo ""

# Cores
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Verificar branch
echo "${YELLOW}📍 Verificando branch...${NC}"
BRANCH=$(git rev-parse --abbrev-ref HEAD)
if [ "$BRANCH" != "feature/fase-6-admin-log" ]; then
  echo "${RED}❌ Você não está na branch 'feature/fase-6-admin-log'!${NC}"
  echo "Execute: git checkout feature/fase-6-admin-log"
  exit 1
fi
echo "${GREEN}✅ Branch correta: $BRANCH${NC}"
echo ""

# 1. BACKEND
echo "${YELLOW}🔧 Backend — Iniciando...${NC}"
echo "Rodando em: http://localhost:5000"
echo ""

# Rodar backend em background
cd /opt/data/ChamadosCamarj
dotnet run --project src/ChamadosCamarj.WebApi &
BACKEND_PID=$!

echo "${GREEN}✅ Backend iniciado (PID: $BACKEND_PID)${NC}"
echo "Aguardando 5 segundos para backend inicializar..."
sleep 5

# Verificar se backend tá rodando
if ! kill -0 $BACKEND_PID 2>/dev/null; then
  echo "${RED}❌ Backend falhou ao iniciar!${NC}"
  exit 1
fi

echo ""
echo "${YELLOW}🎨 Frontend — Verifique as instruções abaixo${NC}"
echo ""

# Mostrar informações
echo "╔═════════════════════════════════════════════════════════════╗"
echo "║                    🎯 TESTES DISPONÍVEIS                   ║"
echo "╚═════════════════════════════════════════════════════════════╝"
echo ""
echo "📌 BACKEND:"
echo "   URL: http://localhost:5000"
echo "   Swagger/API Docs: http://localhost:5000/swagger"
echo "   Status: ${GREEN}✅ Rodando${NC}"
echo ""
echo "📌 FRONTEND:"
echo "   Instruções:"
echo "   1. Abra OUTRA ABA do terminal"
echo "   2. cd /opt/data/ChamadosCamarj/src/ChamadosCamarj.Web"
echo "   3. npm run dev"
echo "   4. Acesse: http://localhost:3000"
echo ""
echo "╔═════════════════════════════════════════════════════════════╗"
echo "║              🧪 GUIA DE TESTES (6 testes)                  ║"
echo "╚═════════════════════════════════════════════════════════════╝"
echo ""
echo "Leia o guia completo:"
echo "   cat .specs/FRONTEND-FASE-6-TESTES.md"
echo ""
echo "Testes:"
echo "  [T1] Reatribuição de Chamado"
echo "  [T2] Alterar Prioridade"
echo "  [T3] Histórico (Timeline)"
echo "  [T4] Comentários Internos"
echo "  [T5] Filtro por Perfil"
echo "  [T6] Status em Tempo Real (SignalR)"
echo ""
echo "╔═════════════════════════════════════════════════════════════╗"
echo ""
echo "${YELLOW}⏸️  Pressione CTRL+C para parar o Backend${NC}"
echo ""

# Manter backend rodando
wait $BACKEND_PID
