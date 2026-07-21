#!/usr/bin/env bash
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$REPO_ROOT"

echo "[1/3] Restoring packages"
dotnet restore Republic.sln

echo "[2/3] Building Debug and Release"
dotnet build Republic.sln --configuration Debug --no-restore
dotnet build Republic.sln --configuration Release --no-restore

echo "[3/3] Running tests"
dotnet test Republic.sln --configuration Debug --no-build
