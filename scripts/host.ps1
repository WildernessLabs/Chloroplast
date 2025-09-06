#!/usr/bin/env pwsh
#
# Hosts the docs site for LOCAL TESTING.
# Always disables base path (passes --no-basepath) so site appears at root.
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

# Execute the host command (always with --no-basepath)
Write-Host "Starting host (base path disabled)..."
dotnet run --project $ToolProject -- host --root $DocsRoot --out $DocsOut --no-basepath

if ($LASTEXITCODE -ne 0) {
    Write-Host "Host command failed to start with exit code $LASTEXITCODE." -ForegroundColor Red
}
