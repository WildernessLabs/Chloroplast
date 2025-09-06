#!/usr/bin/env pwsh
#
# Builds the docs site using dotnet run.
#
# This script is intended to be run from the repository root.
# It automatically locates the tool project and the docs folder.
#
$ErrorActionPreference = "Stop"

# Get the script's directory to reliably locate the repo root
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RepoRoot = Resolve-Path "$ScriptDir/.."

# Define project and site paths relative to the repo root
$ToolProject = "$RepoRoot/toolsrc/Chloroplast.Tool/Chloroplast.Tool.csproj"
$DocsRoot = "$RepoRoot/docs"
$DocsOut = "$RepoRoot/docs/out"

Write-Host "Repository Root: $RepoRoot"
Write-Host "Tool Project:    $ToolProject"
Write-Host "Docs Root:       $DocsRoot"
Write-Host "Output Path:     $DocsOut"
Write-Host ""

# Execute the build command
Write-Host "Building docs..."
dotnet run --project $ToolProject -- build --root $DocsRoot --out $DocsOut

if ($LASTEXITCODE -eq 0) {
    Write-Host "Build completed successfully." -ForegroundColor Green
} else {
    Write-Host "Build failed with exit code $LASTEXITCODE." -ForegroundColor Red
}
