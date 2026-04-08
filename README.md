# Dynamics 365 / Dataverse — Console Application Template

A production-ready C# Console Application template for building Dynamics 365 and Dataverse tools. It provides a structured starting point with authentication, logging, a beautiful terminal UI, and a script-driven entity metadata scaffolder.

---

## Prerequisites

| Requirement | Version |
|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download) | 8.0+ |
| [Azure App Registration](https://learn.microsoft.com/en-us/power-apps/developer/data-platform/walkthrough-register-app-azure-active-directory) | OAuth / Entra ID |

You need an **App Registration** in Microsoft Entra ID (Azure AD) with permissions granted for Dynamics 365 / Dataverse.

---

## Quick Start

### 1. Clone & Navigate

```bash
git clone https://github.com/naked-on-the-bus/Dynamics-Templates.git
cd Dynamics-Templates/console-application
```

### 2. Create Your Configuration

Copy the example config and fill in your environment details:

```bash
cp console/config.example.yaml console/config.yaml
```

Edit `config.yaml`:

```yaml
Dynamics:
  AuthType: OAuth
  Url: https://your-environment.crm.dynamics.com
  AppId: your-app-registration-client-id
  RedirectUri: http://localhost
  LoginPrompt: Auto
```

> **⚠ Security:** `config.yaml` is git-ignored. **Never commit it** — it contains your App Registration ID and environment URL.

### 3. Generate Constants (first-time setup)

Run the scaffold script once to pull your Dataverse schema and generate `Constants.cs`:

```powershell
.\generate-constants.ps1
```

This connects to your environment, fetches all entity metadata, and writes a single `console/Constants.cs` file. Re-run it whenever your schema changes.

### 4. Write Your Business Logic

Open `console/Program.cs` and implement your logic below the marker:

```csharp
// ── Your business logic here ──────────────────────────────────────────────────
```

### 5. Run

```bash
dotnet run --project console
```

---

## Project Structure

```
console-application/
├── generate-constants.ps1      # Run this to pull/refresh Constants.cs from Dataverse
├── console/
│   ├── Program.cs              # Entry point — put your business logic here
│   ├── config.yaml             # Your local config (git-ignored)
│   ├── config.example.yaml     # Config template (committed)
│   ├── Constants.cs            # (generated) All entity & attribute constants — git-ignored
│   ├── console.csproj
│   ├── template-tool/
│   │   ├── AppRunner.cs        # Handles config, logging, banner, and Dataverse connection
│   │   ├── ConnectionBuilder.cs# Builds the Dynamics connection string from config keys
│   │   └── MetadataScaffolder.cs # Fetches schema and generates Constants.cs
│   └── logs/                   # (generated) Serilog rolling log files — git-ignored
└── console-application.slnx
```

---

## NuGet Packages

| Package | Purpose |
|---|---|
| `Microsoft.PowerPlatform.Dataverse.Client` | `ServiceClient` for connecting to Dynamics 365 / Dataverse |
| `Microsoft.Extensions.Configuration` | Configuration abstractions (`ConfigurationBuilder`) |
| `NetEscapades.Configuration.Yaml` | YAML configuration provider (`AddYamlFile`) |
| `Serilog` | Structured logging framework |
| `Serilog.Sinks.File` | Rolling file log sink (`logs/log.txt`) |
| `Spectre.Console` | Rich terminal UI (FigletText, Panels, Status spinners) |

---

## How It Works

### Normal Run (`dotnet run`)

`Program.cs` follows this sequence every time you run:

1. **Read `config.yaml`** — Loaded via `ConfigurationBuilder` + `NetEscapades.Configuration.Yaml`.
2. **Build Connection String** — `ConnectionBuilder` concatenates the YAML keys into a valid Dynamics 365 connection string.
3. **Initialize Serilog** — Configured to write rolling log files to `logs/log.txt`.
4. **Display Welcome Banner** — A `FigletText` ASCII title and a styled `Panel` via Spectre.Console.
5. **Connect** — Creates a `ServiceClient` using the built connection string.
6. **Verify Connection** — Checks `service.IsReady`. On failure: logs the error, displays a red error message, and exits. On success: displays a green panel with the connected environment URL.
7. **Execute Business Logic** — Runs whatever you have written below the `// ── Your business logic here ──` marker.

### Generating Constants (`.\generate-constants.ps1`)

The scaffold script is completely separate from the main program. Run it independently when you need to pull or refresh your entity constants:

```
generate-constants.ps1
  └── dotnet run --project console -- --scaffold
        └── AppRunner.Connect()      (authenticates, shows banner)
        └── MetadataScaffolder.Scaffold()
              └── RetrieveAllEntitiesRequest  (fetches full schema)
              └── Writes console/Constants.cs
```

`MetadataScaffolder` writes **one single file** — `Constants.cs` — directly into the project source folder. It is automatically compiled into your project on the next build.

**Example of what Constants.cs contains:**

```csharp
namespace Constants;

public static class Account
{
    public const string EntityLogicalName = "account";
    public const string Accountid = "accountid";
    public const string Name = "name";
    public const string Emailaddress1 = "emailaddress1";
    // ... all attributes
}

public static class Contact
{
    public const string EntityLogicalName = "contact";
    // ...
}

// ... all entities
```

These constants let you reference entity and attribute logical names without magic strings:

```csharp
var query = new QueryExpression(Account.EntityLogicalName);
query.ColumnSet.AddColumn(Account.Name);
query.ColumnSet.AddColumn(Account.Emailaddress1);
```

> **Note:** `Constants.cs` is git-ignored because it is generated from your specific environment's schema. Each developer runs `generate-constants.ps1` locally after setup.

### Adding Your Business Logic

Open `Program.cs` and write directly after `AppRunner.Connect()` returns:

```csharp
var (service, logger) = AppRunner.Connect();
if (service is null) return;

// ── Your business logic here ──────────────────────────────────────────────────
var query = new QueryExpression(Account.EntityLogicalName);
query.ColumnSet.AddColumn(Account.Name);
var results = service.RetrieveMultiple(query);

foreach (var record in results.Entities)
    logger.Information("Account: {Name}", record[Account.Name]);

Log.CloseAndFlush();
service.Dispose();
```

---

## Configuration Reference

| Key | Description | Example |
|---|---|---|
| `Dynamics:AuthType` | Authentication type | `OAuth` |
| `Dynamics:Url` | Dataverse environment URL | `https://org.crm.dynamics.com` |
| `Dynamics:AppId` | Entra ID App Registration Client ID | `51f81489-...` |
| `Dynamics:RedirectUri` | OAuth redirect URI | `http://localhost` |
| `Dynamics:LoginPrompt` | Login prompt behavior (`Auto`, `Always`, `Never`) | `Auto` |

---

## Security Notes

- **`config.yaml`** is listed in `.gitignore` — it is never committed to source control.
- **`config.example.yaml`** is committed as a template with placeholder values.
- The **AppId** (Client ID) is not a secret by itself, but keeping environment details out of source control is a best practice.
- If you use a **Client Secret** or **Certificate** for non-interactive auth, add those keys to `config.yaml` and ensure they remain git-ignored.
- **`Constants.cs`** is git-ignored because generated files may reveal your schema structure.
- The **`logs/`** folder is git-ignored to prevent accidental commit of runtime log data.

---

## License

This template is provided as-is for internal tooling use.

