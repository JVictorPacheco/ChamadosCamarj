#!/usr/bin/env pwsh
# Script para pegar a branch da Fase 6 e testar (Windows)
# Executar no PowerShell: .\get-fase6.ps1

param(
    [switch]$SkipTests = $false,
    [string]$TestFilter = "all"
)

$ErrorActionPreference = "Continue"

# Cores
$Colors = @{
    Cyan = [System.ConsoleColor]::Cyan
    Green = [System.ConsoleColor]::Green
    Red = [System.ConsoleColor]::Red
    Yellow = [System.ConsoleColor]::Yellow
}

function Write-Step {
    param([string]$Message)
    Write-Host "╔════════════════════════════════════════════════════════════════╗" -ForegroundColor $Colors.Cyan
    Write-Host "║ $Message" -ForegroundColor $Colors.Cyan
    Write-Host "╚════════════════════════════════════════════════════════════════╝" -ForegroundColor $Colors.Cyan
    Write-Host ""
}

function Write-Success {
    param([string]$Message)
    Write-Host "✅ $Message" -ForegroundColor $Colors.Green
}

function Write-Error-Custom {
    param([string]$Message)
    Write-Host "❌ $Message" -ForegroundColor $Colors.Red
}

function Write-Info {
    param([string]$Message)
    Write-Host "📋 $Message" -ForegroundColor $Colors.Yellow
}

# PASSO 1: Verificar git
Write-Step "PASSO 1: Verificar Git"

try {
    $gitVersion = git --version
    Write-Success "Git encontrado: $gitVersion"
}
catch {
    Write-Error-Custom "Git não encontrado! Instale git primeiro."
    exit 1
}
Write-Host ""

# PASSO 2: Verificar branch atual
Write-Step "PASSO 2: Verificar Branch Atual"

$currentBranch = git rev-parse --abbrev-ref HEAD
Write-Info "Você está na branch: $currentBranch"
Write-Host ""

# PASSO 3: Fazer fetch
Write-Step "PASSO 3: Atualizar Lista de Branches"

Write-Info "Sincronizando com repositório remoto..."
git fetch origin
Write-Success "Branches atualizadas!"
Write-Host ""

# PASSO 4: Listar branches
Write-Step "PASSO 4: Branches Disponíveis"

Write-Host "Branches locais:"
git branch
Write-Host ""
Write-Host "Branch remota (Fase 6):"
git branch -r | Select-String "fase-6"
Write-Host ""

# PASSO 5: Checkout
Write-Step "PASSO 5: Fazendo Checkout da Branch"

$branchExists = git branch -a | Select-String "feature/fase-6-admin-log"

if ($branchExists) {
    Write-Info "Branch encontrada. Fazendo checkout..."
    git checkout feature/fase-6-admin-log
    git pull origin feature/fase-6-admin-log
} else {
    Write-Info "Criando branch local..."
    git checkout -b feature/fase-6-admin-log origin/feature/fase-6-admin-log
}

Write-Success "Você está agora na branch: feature/fase-6-admin-log"
Write-Host ""

# PASSO 6: Mostrar commits
Write-Step "PASSO 6: Commits da Fase 6"

Write-Host "Últimos 8 commits:"
git log --oneline -8
Write-Host ""

# PASSO 7: Arquivos modificados
Write-Step "PASSO 7: Arquivos Modificados"

Write-Host "Resumo de mudanças:"
$stat = git diff develop --stat | Select-Object -Last 5
Write-Host $stat
Write-Host ""

# PASSO 8: Restaurar dependências
Write-Step "PASSO 8: Restaurar Dependências"

try {
    $dotnetVersion = dotnet --version
    Write-Info "Executando: dotnet restore"
    dotnet restore
    Write-Success "Dependências restauradas!"
}
catch {
    Write-Error-Custom ".NET não encontrado. Instale .NET SDK primeiro!"
    exit 1
}
Write-Host ""

# PASSO 9: Build
Write-Step "PASSO 9: Compilar Projeto"

Write-Info "Compilando solução..."
dotnet build -c Release

if ($LASTEXITCODE -eq 0) {
    Write-Success "Build bem-sucedido!"
}
else {
    Write-Error-Custom "Build falhou! Verifique os erros acima."
    exit 1
}
Write-Host ""

# PASSO 10: Testes
if (-not $SkipTests) {
    Write-Step "PASSO 10: Executar Testes"

    Write-Host "Escolha uma opção:"
    Write-Host "1) Rodar TODOS os testes"
    Write-Host "2) Rodar apenas testes da Fase 6"
    Write-Host "3) Rodar DEMO test (impressionar a esposa)"
    Write-Host "4) Pular testes agora"
    Write-Host ""
    $choice = Read-Host "Digite a opção (1-4)"

    switch ($choice) {
        "1" {
            Write-Info "Rodando todos os testes..."
            dotnet test tests/ChamadosCamarj.UnitTests/ -v q
        }
        "2" {
            Write-Info "Rodando testes da Fase 6..."
            dotnet test tests/ChamadosCamarj.UnitTests/ -v q --filter "Reatribuir|AlterarPriori|Historico|Fase"
        }
        "3" {
            Write-Info "Rodando DEMO test..."
            dotnet test tests/ChamadosCamarj.UnitTests/ -v d --filter "Demo"
        }
        "4" {
            Write-Info "Pulando testes por enquanto..."
        }
        default {
            Write-Error-Custom "Opção inválida!"
            exit 1
        }
    }
    Write-Host ""
}

# SUCESSO!
Write-Step "✨ SETUP COMPLETO!"

Write-Host ""
Write-Host "╔════════════════════════════════════════════════════════════════╗" -ForegroundColor $Colors.Green
Write-Host "║           🎉 VOCÊ ESTÁ PRONTO PARA TESTAR!                   ║" -ForegroundColor $Colors.Green
Write-Host "╚════════════════════════════════════════════════════════════════╝" -ForegroundColor $Colors.Green
Write-Host ""

Write-Host "📋 Próximos passos:" -ForegroundColor $Colors.Yellow
Write-Host ""
Write-Host "1️⃣  Ver documentação da Fase 6:"
Write-Host "   cat .specs/FASE-6-TESTES.md"
Write-Host ""
Write-Host "2️⃣  Ver explicação do projeto (mostrar pra esposa):"
Write-Host "   cat docs/EXPLICACAO-PARA-ESPOSA.md"
Write-Host ""
Write-Host "3️⃣  Rodar o DEMO test de novo:"
Write-Host "   dotnet test --filter Demo"
Write-Host ""
Write-Host "4️⃣  Testar os endpoints via Postman/Thunder Client"
Write-Host ""
Write-Host "5️⃣  Se tudo OK, volta pra develop e faz PR"
Write-Host "   git checkout develop"
Write-Host "   git pull origin develop"
Write-Host ""
