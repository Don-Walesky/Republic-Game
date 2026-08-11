# PowerShell build & launch script for Republic CLI application
Param(
    [string]$Configuration = "Release",
    [string]$OutputDirectory = "./artifacts/publish"
)

Write-Host "==============================================================" -ForegroundColor Cyan
Write-Host "          REPUBLIC - STANDALONE CLI BUILD SCRIPT               " -ForegroundColor Cyan
Write-Host "==============================================================" -ForegroundColor Cyan

Write-Host "Building Republic solution ($Configuration)..." -ForegroundColor Yellow
dotnet build --configuration $Configuration

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed with errors." -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host "Publishing Republic CLI executable..." -ForegroundColor Yellow
dotnet publish src/Republic.Cli/Republic.Cli.csproj --configuration $Configuration --output $OutputDirectory

if ($LASTEXITCODE -eq 0) {
    Write-Host "Publish successful! Executable published to: $OutputDirectory" -ForegroundColor Green
} else {
    Write-Host "Publish failed." -ForegroundColor Red
}
