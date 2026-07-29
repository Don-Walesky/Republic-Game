# Development

## Requirements

- .NET SDK 8+
- Bash or equivalent shell for `build.sh`

## Setup

```bash
dotnet restore Republic.sln
```

## Build

```bash
dotnet build Republic.sln --configuration Debug
dotnet build Republic.sln --configuration Release
```

## Test

```bash
dotnet test Republic.sln --configuration Debug --no-build
```

## Run Bootstrap Host

```bash
dotnet run --project src/Republic.App/Republic.App.csproj
```

## Repository Conventions

- keep simulation code in `src/Republic.Core`
- keep host/bootstrap code in `src/Republic.App`
- keep tests in `tests/Republic.Core.Tests`
- keep Unity-facing work out of Wave 0 runtime code
