# Dynamics 365 / Dataverse — Console Application Template

![C#](https://img.shields.io/badge/C%23-12-239120?logo=csharp&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)
![PowerShell](https://img.shields.io/badge/PowerShell-5.1+-5391FE?logo=powershell&logoColor=white)
![Dynamics 365](https://img.shields.io/badge/Dynamics%20365-Dataverse-0B53CE?logo=dynamics365&logoColor=white)

A template for quickly scaffolding multiple Dynamics 365 / Dataverse console applications that share the same connection tooling and auto-generated entity constants.

---

## Project Structure

```
Dynamics-Templates/
  config.yaml                ← your Dynamics 365 credentials (shared by all apps)
  generate-constants.ps1     ← generates entity constants from Dataverse
  console-application.slnx   ← solution file
  shared/                    ← shared class library (AppRunner, ConnectionBuilder)
  constants/                 ← auto-generated entity constants (class library)
  console/                   ← template console app (copy this for new apps)
  scripts/RetrieveConstants/ ← the metadata generator tool
```

---

## Libraries

| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.PowerPlatform.Dataverse.Client | 1.1.22 | Dynamics 365 / Dataverse SDK |
| Microsoft.Extensions.Configuration | 8.0.0 | Configuration binding |
| NetEscapades.Configuration.Yaml | 3.1.0 | YAML config file support |
| Serilog | 4.2.0 | Structured logging |
| Serilog.Sinks.File | 6.0.0 | File log sink |
| Spectre.Console | 0.49.1 | Terminal UI (banners, panels, status) |

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- A Dynamics 365 / Dataverse environment with an Azure AD App Registration (OAuth)

---

## Getting Started

### 1. Clone the repo

```bash
git clone <your-repo-url>
cd Dynamics-Templates
```

### 2. Configure your environment

Edit `config.yaml` at the root with your Dynamics 365 credentials:

```yaml
Dynamics:
  AuthType: OAuth
  Url: https://yourorg.crm4.dynamics.com
  AppId: your-app-id
  RedirectUri: http://localhost
  LoginPrompt: Auto
```

### 3. Generate entity constants

```powershell
.\generate-constants.ps1
```

This connects to your Dataverse environment, retrieves all entity metadata, and generates one `.cs` file per entity inside `constants/`. All console apps reference this project automatically.

### 4. Run the template app

```powershell
dotnet run --project console
```

---

## Creating a New Console App

1. Copy the `console/` folder:

```powershell
Copy-Item -Recurse console my-new-app
```

2. Rename the `.csproj` inside:

```powershell
Rename-Item my-new-app/console.csproj my-new-app.csproj
```

3. Add it to the solution:

```powershell
dotnet sln console-application.slnx add my-new-app/my-new-app.csproj
```

4. Write your logic in `my-new-app/Program.cs` — connection, logging, constants, and config are already wired up.

---

## How It Works

- **`shared/`** — Contains `AppRunner` (reads config, connects, sets up logging) and `ConnectionBuilder`. Referenced by all console apps.
- **`constants/`** — A class library populated by `generate-constants.ps1`. Each entity becomes a static class with `const string` fields for every attribute. Referenced by all console apps.
- **`config.yaml`** — Single config file at the root. Copied to each app's output directory at build time via the `Link` item in the `.csproj`.
- **`console/`** — A minimal working app. Copy it to create new apps. The `.csproj` already references `shared/` and `constants/`.
