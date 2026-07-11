#!/usr/bin/env pwsh
# Script para rodar Backend + Frontend (Windows)
# Executar: .\start-fase6.ps1

param(
    [switch]$SkipBackend = $false
)

Write-Host "╔═════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║     🚀 INICIANDO FASE 6 — Backend + Frontend               ║" -ForegroundColor Cyan
Write-Host "╚═════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Verificar branch
Write-Host "📍 Verificando branch..." -ForegroundColor Yellow
$branch = & git rev-parse --abbrev-ref HEAD

if ($branch -ne "feature/fase-6-admin-log") {
    Write-Host "❌ Você não está na branch 'feature/fase-6-admin-log'!" -ForegroundColor Red
    Write-Host "Execute: git checkout feature/fase-6-admin-log"
    exit 1
}
Write-Host "✅ Branch correta: $branch" -ForegroundColor Green
Write-Host ""

if (-not $SkipBackend) {
    # BACKEND
    Write-Host "🔧 Backend — Iniciando..." -ForegroundColor Yellow
    Write-Host "Rodando em: http://localhost:5000"
    Write-Host ""

    # Abrir nova janela do PowerShell para o backend
    $backendScript = @"
        cd "$PWD"
        Write-Host "Iniciando Backend..."
        dotnet run --project src/ChamadosCamarj.WebApi
    "@

    Start-Process powershell -ArgumentList "-NoExit", "-Command", $backendScript

    Write-Host "✅ Backend iniciado em nova janela" -ForegroundColor Green
    Start-Sleep -Seconds 3
}

Write-Host ""
Write-Host "╔═════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║                    🎯 TESTES DISPONÍVEIS                   ║" -ForegroundColor Cyan
Write-Host "╚═════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

Write-Host "📌 BACKEND:" -ForegroundColor Green
Write-Host "   URL: http://localhost:5000"
Write-Host "   Swagger/API Docs: http://localhost:5000/swagger"
Write-Host "   Status: ✅ Rodando"
Write-Host ""

Write-Host "📌 FRONTEND:" -ForegroundColor Green
Write-Host "   Instruções:"
Write-Host "   1. Abra OUTRO PowerShell"
Write-Host "   2. cd src/ChamadosCamarj.Web"
Write-Host "   3. npm run dev"
Write-Host "   4. Acesse: http://localhost:3000"
Write-Host ""

Write-Host "╔═════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║              🧪 GUIA DE TESTES (6 testes)                  ║" -ForegroundColor Cyan
Write-Host "╚═════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

Write-Host "Leia o guia completo:" -ForegroundColor Yellow
Write-Host "   cat .specs/FRONTEND-FASE-6-TESTES.md"
Write-Host ""
Write-Host "Testes:"
Write-Host "  [T1] Reatribuição de Chamado"
Write-Host "  [T2] Alterar Prioridade"
Write-Host "  [T3] Histórico (Timeline)"
Write-Host "  [T4] Comentários Internos"
Write-Host "  [T5] Filtro por Perfil"
Write-Host "  [T6] Status em Tempo Real (SignalR)"
Write-Host ""

Write-Host "╔═════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host ""
