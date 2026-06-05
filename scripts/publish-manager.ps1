param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64"
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$publishRoot = Join-Path $repoRoot "release\desktop-manager"
$resolvedRepoRoot = [System.IO.Path]::GetFullPath($repoRoot)
$resolvedPublishRoot = [System.IO.Path]::GetFullPath($publishRoot)
$expectedReleaseRoot = [System.IO.Path]::GetFullPath((Join-Path $resolvedRepoRoot "release"))
$publishedExe = Join-Path $resolvedPublishRoot "AnalictY.Manager.exe"

if (-not $resolvedPublishRoot.StartsWith($expectedReleaseRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "Pasta de publicacao fora do diretorio release: $resolvedPublishRoot"
}

Get-Process -Name "AnalictY.Manager" -ErrorAction SilentlyContinue |
    Where-Object { $_.Path -eq $publishedExe } |
    Stop-Process -Force

if (Test-Path $publishRoot) {
    $removed = $false

    for ($attempt = 1; $attempt -le 5; $attempt++) {
        try {
            Remove-Item -LiteralPath $resolvedPublishRoot -Recurse -Force
            $removed = $true
            break
        }
        catch {
            if ($attempt -eq 5) {
                throw "Nao foi possivel limpar a pasta de publicacao. Feche o AnalictY.Manager aberto em release\desktop-manager e tente novamente. Detalhes: $($_.Exception.Message)"
            }

            Start-Sleep -Milliseconds 500
        }
    }
}

New-Item -ItemType Directory -Path $resolvedPublishRoot -Force | Out-Null

dotnet publish `
    (Join-Path $repoRoot "src\AnalictY.Manager\AnalictY.Manager.csproj") `
    -c $Configuration `
    -r $Runtime `
    --self-contained false `
    -o $resolvedPublishRoot

Write-Host "Publicacao concluida em: $resolvedPublishRoot"
