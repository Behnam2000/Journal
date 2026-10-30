# Project Structure

This repository is a .NET solution for a personal trading journal application. It follows a layered architecture with a domain model, application layer, infrastructure, API, and client projects.

Generated folders such as `bin/` and `obj/` are omitted for clarity.

```text
Journal/
├── README.md
├── Journal.slnx
├── Directory.Build.props
├── Directory.Packages.props
├── journal_option_a_triggers.sql
├── journal_option_b_views.sql
├── src/
│   ├── Clients/
│   │   ├── Journal.SharedUI/
│   │   │   ├── Component1.razor
│   │   │   ├── Component1.razor.css
│   │   │   ├── ExampleJsInterop.cs
│   │   │   ├── _Imports.razor
│   │   │   ├── Journal.SharedUI.csproj
│   │   │   └── wwwroot/
│   │   │       ├── background.png
│   │   │       └── exampleJsInterop.js
│   │   └── Journal.Web/
│   │       ├── Components/
│   │       │   ├── App.razor
│   │       │   ├── Routes.razor
│   │       │   ├── _Imports.razor
│   │       │   └── Layout/
│   │       │       └── (layout files)
│   │       │   └── Pages/
│   │       │       └── (page components)
│   │       ├── wwwroot/
│   │       │   ├── app.css
│   │       │   ├── favicon.png
│   │       │   └── lib/
│   │       ├── appsettings.json
│   │       ├── appsettings.Development.json
│   │       ├── Program.cs
│   │       ├── Journal.Web.csproj
│   │       └── Properties/
│   │           └── launchSettings.json
│   ├── Core/
│   │   ├── Journal.Application/
│   │   │   ├── Auth/
│   │   │   │   └── Commands/
│   │   │   ├── Journals/
│   │   │   │   ├── Commands/
│   │   │   │   └── Queries/
│   │   │   ├── Trades/
│   │   │   │   ├── Commands/
│   │   │   │   └── Queries/
│   │   │   ├── Common/
│   │   │   │   └── Interfaces/
│   │   │   ├── DependencyInjection.cs
│   │   │   ├── GlobalUsings.cs
│   │   │   └── Journal.Application.csproj
│   │   └── Journal.Domain/
│   │       ├── Common/
│   │       │   └── Interfaces/
│   │       ├── DailyJournal.cs
│   │       ├── Trade.cs
│   │       └── Journal.Domain.csproj
│   ├── Infrastructure/
│   │   └── Journal.Infrastructure/
│   │       ├── Identity/
│   │       │   ├── IdentityService.cs
│   │       │   └── RefreshToken.cs
│   │       ├── Persistence/
│   │       │   ├── AppDbContext.cs
│   │       │   └── Configurations/
│   │       ├── Migrations/
│   │       │   ├── (timestamped migration files)
│   │       │   └── AppDbContextModelSnapshot.cs
│   │       ├── DependencyInjection.cs
│   │       ├── GlobalUsings.cs
│   │       └── Journal.Infrastructure.csproj
│   └── Presentation/
│       └── Journal.Api/
│           ├── Controllers/
│           │   ├── AuthController.cs
│           │   ├── DailyJournalsController.cs
│           │   └── TradesController.cs
│           ├── Services/
│           │   └── CurrentUserService.cs
│           ├── appsettings.json
│           ├── appsettings.Development.json
│           ├── Journal.Api.csproj
│           ├── Journal.Api.http
│           ├── Program.cs
│           └── Properties/
│               └── launchSettings.json
└── .git/ (Git metadata, not part of app source)
```

## Layer overview

- `src/Core/Journal.Domain` – domain entities and business rules.
- `src/Core/Journal.Application` – application services, commands, queries, interfaces, and DI registration.
- `src/Infrastructure/Journal.Infrastructure` – EF Core persistence, identity, migrations, and infrastructure services.
- `src/Presentation/Journal.Api` – ASP.NET Core REST API endpoints.
- `src/Clients/Journal.Web` – Blazor web client UI.
- `src/Clients/Journal.SharedUI` – shared UI components for client apps.

## Root-level files

- `README.md` – project overview.
- `Journal.slnx` – solution definition.
- `Directory.Build.props` and `Directory.Packages.props` – central build/package configuration.
- `journal_option_a_triggers.sql` and `journal_option_b_views.sql` – database-related SQL scripts.
