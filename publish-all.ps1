#!/usr/bin/env pwsh
# Publishes dangit for all supported platforms.
# CI handles official builds; this script is for local dev convenience.

param(
    [string]$Configuration = "Release",
    [string]$OutputRoot = "artifacts"
)

$ErrorActionPreference = "Stop"

$rids = @(
    "win-x64",
    "win-arm64",
    "linux-x64",
    "linux-arm64",
    "osx-x64",
    "osx-arm64"
)

$project = "src/Dangit.Cli/Dangit.Cli.fsproj"

foreach ($rid in $rids) {
    $outDir = Join-Path $OutputRoot $rid
    Write-Host "Publishing $rid -> $outDir" -ForegroundColor Cyan
    dotnet publish $project -c $Configuration -r $rid -o $outDir
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Publish failed for $rid"
        exit 1
    }
}

Write-Host "`nAll platforms published to $OutputRoot" -ForegroundColor Green
