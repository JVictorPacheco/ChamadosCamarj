#!/usr/bin/env pwsh
# Script de Testes - Fase 6 (ChamadosCamarj)
# Execute no Windows: .\run-tests-fase6.ps1

param(
    [string]$Mode = "all",
    [switch]$Coverage = $false,
    [switch]$Verbose = $false
)

$ErrorActionPreference = "Stop"
$Colors = @{
    Green = [System.ConsoleColor]::Green
    Red = [System.ConsoleColor]::Red
    Yellow = [System.ConsoleColor]::Yellow
    Cyan = [System.ConsoleColor]::Cyan
}

function Write-ColorOutput {
    param([string]$Message, [string]$Color = "Cyan")
    Write-Host $Message -ForegroundColor $Colors[$Color]
}

function Test-DotnetInstalled {
    try {
        $null = dotnet --version
        Write-ColorOutput "✅ dotnet encontrado: $(dotnet --version)" "Green"
    }
    catch {
        Write-ColorOutput "❌ dotnet não está instalado! Instale de https://dotnet.microsoft.com/download" "Red"
        exit 1
    }
}

Write-ColorOutput "═════════════════════════════════════════════════════" "Cyan"
Write-ColorOutput "   🧪 TESTES UNITÁRIOS - FASE 6 (ChamadosCamarj)   " "Cyan"
Write-ColorOutput "═════════════════════════════════════════════════════" "Cyan"
Write-Host ""

# Verificar dotnet
Test-DotnetInstalled

# Caminhos
$ProjectPath = (Get-Location).Path
$TestProjectPath = "$ProjectPath\tests\ChamadosCamarj.UnitTests\ChamadosCamarj.UnitTests.csproj"

if (!(Test-Path $TestProjectPath)) {
    Write-ColorOutput "❌ Projeto de testes não encontrado em: $TestProjectPath" "Red"
    exit 1
}

Write-ColorOutput "📂 Projeto: $TestProjectPath" "Cyan"
Write-Host ""

# Modo de teste
$TestFilter = switch ($Mode) {
    "fase6" { '--filter "Reatribuir|AlterarPriori|Historico|Fase"' }
    "handlers" { '--filter "HandlerTests"' }
    "validators" { '--filter "ValidatorTests"' }
    default { "" }
}

$VerboseFlag = if ($Verbose) { "-v d" } else { "-v q" }
$CoverageFlag = if ($Coverage) { "/p:CollectCoverage=true /p:CoverageFormat=opencover" } else { "" }

# Build
Write-ColorOutput "🔨 Compilando projeto..." "Yellow"
$BuildCmd = "dotnet build --configuration Release"
Invoke-Expression $BuildCmd
if ($LASTEXITCODE -ne 0) {
    Write-ColorOutput "❌ Build falhou!" "Red"
    exit 1
}

Write-ColorOutput "✅ Build bem-sucedido!" "Green"
Write-Host ""

# Testes
Write-ColorOutput "🧪 Executando testes..." "Yellow"
$TestCmd = "dotnet test `"$TestProjectPath`" $VerboseFlag $TestFilter $CoverageFlag --no-build"
Invoke-Expression $TestCmd

if ($LASTEXITCODE -eq 0) {
    Write-ColorOutput "✅ TODOS OS TESTES PASSARAM!" "Green"
} else {
    Write-ColorOutput "❌ Alguns testes falharam!" "Red"
    exit 1
}

Write-Host ""
Write-ColorOutput "═════════════════════════════════════════════════════" "Cyan"
Write-ColorOutput "   🎉 Fase 6 Backend Pronta para Review!           " "Cyan"
Write-ColorOutput "═════════════════════════════════════════════════════" "Cyan"

Write-Host ""
Write-ColorOutput "📋 Próximos Passos:" "Yellow"
Write-Host "1. Revisar código da branch: git log --oneline feature/fase-6-admin-log ^develop"
Write-Host "2. Rodar testes manuais via Postman/Thunder Client"
Write-Host "3. Iniciar Fase 6 Frontend quando aprovado"
Write-Host ""
