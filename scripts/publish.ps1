# Win-Arma — Release Build Script
# Kullanım / Usage: .\scripts\publish.ps1 [-Version "1.0.0"]

param(
    [string]$Version = "1.0.0"
)

$ErrorActionPreference = "Stop"

$ProjectRoot = Split-Path -Parent $PSScriptRoot
$PublishDir  = Join-Path $ProjectRoot "publish"
$ZipName     = "WinArma-v$Version.zip"
$ZipPath     = Join-Path $ProjectRoot $ZipName

Write-Host ""
Write-Host "╔══════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║     Win-Arma Release Builder         ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# ── 1. Clean previous publish ────────────────────────────────────────────────
if (Test-Path $PublishDir) {
    Write-Host "🧹 Cleaning previous publish..." -ForegroundColor Yellow
    Remove-Item $PublishDir -Recurse -Force
}
if (Test-Path $ZipPath) {
    Remove-Item $ZipPath -Force
}

# ── 2. Restore ───────────────────────────────────────────────────────────────
Write-Host "📦 Restoring dependencies..." -ForegroundColor Blue
Set-Location $ProjectRoot
dotnet restore
if ($LASTEXITCODE -ne 0) { throw "dotnet restore failed" }

# ── 3. Build ─────────────────────────────────────────────────────────────────
Write-Host "🏗️  Building Release..." -ForegroundColor Blue
dotnet build --configuration Release --no-restore
if ($LASTEXITCODE -ne 0) { throw "dotnet build failed" }

# ── 4. Publish ───────────────────────────────────────────────────────────────
Write-Host "📤 Publishing..." -ForegroundColor Blue
dotnet publish --configuration Release --output $PublishDir --no-restore
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed" }

# ── 5. Package ───────────────────────────────────────────────────────────────
Write-Host "🗜️  Creating archive: $ZipName..." -ForegroundColor Blue
Compress-Archive -Path "$PublishDir\*" -DestinationPath $ZipPath -Force

# ── 6. Done ──────────────────────────────────────────────────────────────────
$Size = [math]::Round((Get-Item $ZipPath).Length / 1MB, 2)
Write-Host ""
Write-Host "✅ Release package ready!" -ForegroundColor Green
Write-Host "   📁 Folder : $PublishDir" -ForegroundColor White
Write-Host "   🗜️  Archive: $ZipPath ($Size MB)" -ForegroundColor White
Write-Host ""
Write-Host "GitHub'a yüklemek için / To upload to GitHub:" -ForegroundColor Gray
Write-Host "  git tag v$Version" -ForegroundColor Gray
Write-Host "  git push origin v$Version" -ForegroundColor Gray
Write-Host ""
