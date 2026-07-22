# IslandJamaica Tours

IslandJamaica Tours is an ASP.NET Web Forms application for customer records,
tour bookings, transportation assignment, invoices, payments, refunds, reports,
user administration, and audit history.

## Technology

- C# and ASP.NET Web Forms
- .NET Framework 4.7.2
- SQL Server
- Bootstrap, HTML, CSS, and JavaScript

## Security and reliability

- Staff, administrator, and client authorization is enforced centrally before
  protected pages execute.
- Passwords are stored with PBKDF2-SHA256. A successful login automatically
  upgrades a legacy plaintext password.
- Persistent sign-in cookies contain an encrypted Forms Authentication ticket,
  use HttpOnly and SameSite=Strict, and are marked Secure when HTTPS is active.
- Login regenerates the session identifier.
- SQL commands use parameters.
- The exchange-rate API key is no longer stored in the tracked Web.config.
- Production compilation disables debug output and suppresses framework version
  headers.

## Requirements

- Windows 10 or later
- Visual Studio 2022 with ASP.NET and web development tools
- .NET Framework 4.7.2 developer pack
- SQL Server Express or SQL Server

## Local setup

1. Clone the repository.
2. Open `invenman/invenman.sln` in Visual Studio.
3. In SQL Server Management Studio, run
   `invenman/invenman/App_Data/TravelTime.sql`.
4. Create the initial administrator from PowerShell:

   ```powershell
   .\tools\New-TravelTimeAdmin.ps1 -Username admin
   ```

   The script prompts for a password of at least 12 characters and writes only
   its PBKDF2 hash to SQL Server.

5. Confirm the `TravelTime` connection string in
   `invenman/invenman/Web.config`. The default targets local
   `SQLEXPRESS` with Windows authentication.
6. Build and run the solution with IIS Express.

The SQL script is safe to run again. It creates missing tables and indexes and
migrates the older attraction and transportation columns used by previous
versions.

## Optional Fixer exchange-rate integration

Use one of these local configuration methods:

- Set the `TRAVELTIME_FIXER_API_KEY` environment variable for IIS Express or
  the application pool.
- Copy
  `invenman/invenman/AppSettings.local.config.example` to
  `invenman/invenman/AppSettings.local.config`, then place your own key in the
  copied file.

`AppSettings.local.config` is ignored by Git. Never commit an API key.

The API key that appeared in an earlier public revision must be revoked and
replaced at the provider; removing it from the current file does not revoke it.

## Roles

- `Admin`: application configuration, audit log and user administration.
- `Staff`: client, attraction, booking, transportation and payment workflows.
- `Client`: the current client's bookings, invoices, receipts and upcoming
  transportation.

## Build verification

GitHub Actions restores packages and builds the solution on Windows for every
pull request and push to `main`.

## Repository hygiene

The repository ignores Visual Studio state, restored packages, build outputs,
database backups, user settings, and local secret configuration.

## Author

Orane Gonzales
