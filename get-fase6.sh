#!/bin/bash
# Script para pegar a branch da Fase 6 e testar
# Executar no seu MacBook: chmod +x get-fase6.sh && ./get-fase6.sh

set -e

echo "╔════════════════════════════════════════════════════════════════╗"
echo "║         🚀 SETUP FASE 6 — ChamadosCamarj                      ║"
echo "╚════════════════════════════════════════════════════════════════╝"
echo ""

# Cores
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Função para print colorido
print_step() {
    echo -e "${CYAN}═══════════════════════════════════════════════════════════════${NC}"
    echo -e "${CYAN}$1${NC}"
    echo -e "${CYAN}═══════════════════════════════════════════════════════════════${NC}"
    echo ""
}

print_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

print_error() {
    echo -e "${RED}❌ $1${NC}"
}

print_info() {
    echo -e "${YELLOW}📋 $1${NC}"
}

# PASSO 1: Verificar git
print_step "PASSO 1: Verificar Git"

if ! command -v git &> /dev/null; then
    print_error "Git não encontrado! Instale git primeiro."
    exit 1
fi

print_success "Git encontrado: $(git --version)"
echo ""

# PASSO 2: Verificar branch atual
print_step "PASSO 2: Verificar Branch Atual"

CURRENT_BRANCH=$(git rev-parse --abbrev-ref HEAD)
print_info "Você está na branch: ${YELLOW}$CURRENT_BRANCH${NC}"
echo ""

# PASSO 3: Fazer fetch de branches remotas
print_step "PASSO 3: Atualizar Lista de Branches"

print_info "Sincronizando com o repositório remoto..."
git fetch origin
print_success "Branches atualizadas!"
echo ""

# PASSO 4: Listar branches
print_step "PASSO 4: Branches Disponíveis"

echo "Branches locais:"
git branch
echo ""
echo "Branch remota (Fase 6):"
git branch -r | grep "fase-6"
echo ""

# PASSO 5: Fazer checkout
print_step "PASSO 5: Fazendo Checkout da Branch"

if git show-ref --quiet refs/heads/feature/fase-6-admin-log; then
    print_info "Branch local já existe. Atualizando..."
    git checkout feature/fase-6-admin-log
    git pull origin feature/fase-6-admin-log
else
    print_info "Criando branch local a partir do remoto..."
    git checkout -b feature/fase-6-admin-log origin/feature/fase-6-admin-log
fi

print_success "Você está agora na branch: feature/fase-6-admin-log"
echo ""

# PASSO 6: Mostrar commits
print_step "PASSO 6: Commits da Fase 6"

echo "Últimos 8 commits:"
git log --oneline -8
echo ""

# PASSO 7: Mostrar arquivos modificados
print_step "PASSO 7: Arquivos Modificados"

echo "Resumo de mudanças:"
git diff develop --stat | tail -5
echo ""

# PASSO 8: Restaurar dependências
print_step "PASSO 8: Restaurar Dependências"

if command -v dotnet &> /dev/null; then
    print_info "Executando: dotnet restore"
    dotnet restore
    print_success "Dependências restauradas!"
else
    print_error ".NET não encontrado. Instale .NET SDK primeiro!"
    exit 1
fi
echo ""

# PASSO 9: Build
print_step "PASSO 9: Compilar Projeto"

print_info "Compilando solução..."
dotnet build -c Release

if [ $? -eq 0 ]; then
    print_success "Build bem-sucedido!"
else
    print_error "Build falhou! Verifique os erros acima."
    exit 1
fi
echo ""

# PASSO 10: Rodar testes
print_step "PASSO 10: Executar Testes"

echo "Escolha uma opção:"
echo "1) Rodar TODOS os testes"
echo "2) Rodar apenas testes da Fase 6"
echo "3) Rodar DEMO test (impressionar a esposa)"
echo "4) Pular testes agora"
read -p "Digite a opção (1-4): " choice

case $choice in
    1)
        print_info "Rodando todos os testes..."
        dotnet test tests/ChamadosCamarj.UnitTests/ -v q
        ;;
    2)
        print_info "Rodando testes da Fase 6..."
        dotnet test tests/ChamadosCamarj.UnitTests/ -v q --filter "Reatribuir|AlterarPriori|Historico|Fase"
        ;;
    3)
        print_info "Rodando DEMO test..."
        dotnet test tests/ChamadosCamarj.UnitTests/ -v d --filter "Demo"
        ;;
    4)
        print_info "Pulando testes por enquanto..."
        ;;
    *)
        print_error "Opção inválida!"
        exit 1
        ;;
esac
echo ""

# PASSO 11: Sucesso!
print_step "✨ SETUP COMPLETO!"

echo -e "${GREEN}"
echo "╔════════════════════════════════════════════════════════════════╗"
echo "║           🎉 VOCÊ ESTÁ PRONTO PARA TESTAR!                   ║"
echo "╚════════════════════════════════════════════════════════════════╝"
echo -e "${NC}"
echo ""
echo "📋 Próximos passos:"
echo ""
echo "1️⃣  Ver documentação da Fase 6:"
echo "   cat .specs/FASE-6-TESTES.md"
echo ""
echo "2️⃣  Ver explicação do projeto (mostrar pra esposa):"
echo "   cat docs/EXPLICACAO-PARA-ESPOSA.md"
echo ""
echo "3️⃣  Rodar o DEMO test de novo:"
echo "   dotnet test --filter Demo"
echo ""
echo "4️⃣  Testar os endpoints via Postman/Thunder Client"
echo ""
echo "5️⃣  Se tudo OK, volta pra develop e faz PR"
echo "   git checkout develop"
echo "   git pull origin develop"
echo "   [criar PR no GitHub]"
echo ""
