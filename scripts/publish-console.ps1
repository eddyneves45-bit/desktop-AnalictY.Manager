param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64"
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$publishRoot = Join-Path $repoRoot "release\desktop-console"
$resolvedRepoRoot = [System.IO.Path]::GetFullPath($repoRoot)
$resolvedPublishRoot = [System.IO.Path]::GetFullPath($publishRoot)
$expectedReleaseRoot = [System.IO.Path]::GetFullPath((Join-Path $resolvedRepoRoot "release"))

if (-not $resolvedPublishRoot.StartsWith($expectedReleaseRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "Pasta de publicacao fora do diretorio release: $resolvedPublishRoot"
}

if (Test-Path $publishRoot) {
    Remove-Item -LiteralPath $resolvedPublishRoot -Recurse -Force
}

New-Item -ItemType Directory -Path $resolvedPublishRoot -Force | Out-Null

dotnet publish `
    (Join-Path $repoRoot "src\AnalictY.Console\AnalictY.Console.csproj") `
    -c $Configuration `
    -r $Runtime `
    --self-contained false `
    -o $resolvedPublishRoot

Write-Host "Publicacao concluida em: $resolvedPublishRoot"
