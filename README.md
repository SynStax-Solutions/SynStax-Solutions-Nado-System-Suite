# SynStax Solutions – Nado Graphics System Suite

**Nado Graphics Business Management System (BMS)**

A full-stack business management platform built for **Nado Graphics**, covering client management, job cards, quoting, invoicing, suppliers, products, statements, enquiries, and portfolio submissions.

| Component              | Technology                          | Purpose                          |
|------------------------|-------------------------------------|----------------------------------|
| Backend API            | ASP.NET Core Web API (.NET 10)      | REST API + SignalR + PDF generation |
| Frontend               | ASP.NET Core MVC (.NET 10)          | Cookie-authenticated web UI      |
| Database               | SQL Server + EF Core                | Persistent data store            |
| Auth                   | JWT (API) + Cookie (Frontend)       | Secure role-based access         |
| PDF Generation         | QuestPDF                            | Quotes, Invoices, Statements     |
| Real-time              | SignalR                             | Job Card updates                 |
| Portfolio Website      | Vite / static site                  | Public-facing marketing site     |

> **Live Demo Status**  
> The application is currently **not in stable public production**.  
> A deployment exists at:  
> **https://nadographicsbms.runasp.net/**  
> (redirects to `/Account/Login`).  
> This environment is for demonstration / testing only

---

## Features

### Core Business Modules
- **Clients & Organisations** – Full CRUD, contact details, organisation linking
- **Suppliers** – Supplier management
- **Products & Categories** – Product catalogue with categories
- **Job Cards** – Work order tracking with real-time SignalR updates
- **Quotes** – Create, edit, convert to invoices + PDF generation
- **Invoices** – Full invoice lifecycle, payments, PDF generation
- **Statements** – Client account statements + PDF
- **Enquiries** – Incoming enquiry handling
- **Portfolio Submissions** – Designer portfolio uploads / review

### Security & User Management
- Role-based access control:
  - **Manager** (Owner)
  - **PA/Admin**
  - **Graphic Designer**
- JWT authentication on the API
- Cookie authentication on the frontend
- Email verification & password reset flows
- Seeded owner account on first run
- Hierarchical role management (Manager can manage all roles)

### Technical Highlights
- Clean separation: Frontend ↔ Backend API
- Automatic JWT attachment via `ApiTokenHandler`
- QuestPDF for professional document generation
- SignalR hub (`/jobCardHub`)
- Swagger / OpenAPI with Bearer auth
- GitHub Actions CI (build, test, vulnerability scanning)

---

## Solution Structure

```
SynStax-Solutions-Nado-System-Suite/
├── Nado Graphic MS Backend/          # ASP.NET Core Web API
│   ├── Controllers/
│   ├── Services/
│   ├── Models/
│   ├── Data/BMSDbContext.cs
│   ├── Hubs/JobCardHub.cs
│   ├── Security/
│   └── Assets/
├── Nado Graphics MS Frontend/        # ASP.NET Core MVC
│   ├── Controllers/
│   ├── Views/
│   ├── Services/
│   └── wwwroot/
├── nado-graphics-website/            # Public portfolio site (Vite)
├── .github/workflows/
│   ├── ci-build-test.yml
│   └── code-quality.yml
├── SynStax-Solutions-Nado-System-Suite.slnx
└── README.md
```

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server (LocalDB, Express, or full instance)
- Visual Studio 2022 / VS Code / Rider (recommended)
- Git

---

## Local Development Setup

### 1. Clone the repository

```bash
git clone <your-repo-url>
cd SynStax-Solutions-Nado-System-Suite
```

### 2. Configure the Backend

Create / edit `Nado Graphic MS Backend/appsettings.Development.json` (or use User Secrets):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=NadoGraphicsBMS;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "NadoGraphicsBMS",
    "Audience": "NadoGraphicsFrontend"
  },
  "App": {
    "FrontendBaseUrl": "https://localhost:7227"
  },
  "SeedOwner": {
    "Email": "owner@nadographics.local",
    "Password": "ChangeMe123!",
    "FirstName": "Owner",
    "LastName": "Account"
  },
  "Email": {
    // SMTP settings for MailKit (verification, password reset, etc.)
  }
}
```

> **Important**: The `Jwt:Key` **must** be at least 32 characters long or the application will throw on startup.

### 3. Configure the Frontend

Edit `Nado Graphics MS Frontend/appsettings.Development.json`:

```json
{
  "ApiSettings": {
    "BaseUrl": "https://localhost:7xxx/"   // Backend HTTPS port
  }
}
```

### 4. Database

```bash
cd "Nado Graphic MS Backend"
dotnet ef database update
```

(Or let the application create the database on first run if you have the migrations applied.)

### 5. Run the applications

**Terminal 1 – Backend**
```bash
cd "Nado Graphic MS Backend"
dotnet run
```

**Terminal 2 – Frontend**
```bash
cd "Nado Graphics MS Frontend"
dotnet run
```

- Backend Swagger: `https://localhost:<backend-port>/swagger`
- Frontend: `https://localhost:7227` (or the port shown)

Log in with the seeded owner credentials.

---

## Deployment Explanation

The system is **not currently running in a stable public production environment**. The URL below is a temporary / demonstration deployment only.

### Current Deployment
- **URL**: https://nadographicsbms.runasp.net/
- **Hosting**: Free/shared ASP.NET hosting (runasp.net / IIS)
- **Status**: Functional enough to reach the login page, but **not considered production-ready** or permanently available.
- **Limitations of current host**:
  - Cold starts / sleep behaviour on free tiers
  - Limited SQL Server options
  - No guaranteed uptime or custom domain SSL management
  - Difficulty managing secrets and environment-specific configuration

### Recommended Production Architecture

| Layer          | Recommended Service                          |
|----------------|----------------------------------------------|
| Frontend       | Azure App Service / IIS / Linux container    |
| Backend API    | Azure App Service / Container Apps           |
| Database       | Azure SQL Database / SQL Server on VM        |
| Secrets        | Azure Key Vault / Environment Variables      |
| Email          | SendGrid / Amazon SES / SMTP provider        |
| Files / PDFs   | Azure Blob Storage (optional)                |
| CI/CD          | GitHub Actions → Azure / Docker Hub          |

### Production Deployment Steps (High Level)

1. **Prepare SQL Server**
   - Create production database
   - Apply EF Core migrations (`dotnet ef database update --connection "..."`)

2. **Configure Backend**
   - Set environment variables / `appsettings.Production.json`:
     - `ConnectionStrings__DefaultConnection`
     - `Jwt__Key` (strong random ≥ 32 chars)
     - `App__FrontendBaseUrl` = production frontend URL
     - SMTP settings
     - Seed owner credentials (only for first run)

3. **Configure Frontend**
   - `ApiSettings__BaseUrl` = production backend URL
   - Ensure cookie settings (`SecurePolicy = Always`, correct domain)

4. **CORS**
   - Update the Backend CORS policy to allow the production frontend origin.

5. **Publish**
   ```bash
   # Backend
   dotnet publish "Nado Graphic MS Backend" -c Release -o ./publish/backend

   # Frontend
   dotnet publish "Nado Graphics MS Frontend" -c Release -o ./publish/frontend
   ```

6. **Deploy** the published folders to your hosting provider (Azure App Service, IIS, Docker, etc.).

7. **HTTPS & Domain**
   - Enforce HTTPS
   - Configure custom domain + certificate

8. **Post-deployment**
   - Verify seeded Manager account
   - Test login, job cards (SignalR), PDF generation, email flows

### Docker (Optional Future Improvement)
A multi-stage Dockerfile for both Frontend and Backend + SQL Server container would make the system fully portable. Currently the repository does not ship Dockerfiles.

---

## Roles & Permissions Summary

| Role              | Capabilities                                      |
|-------------------|---------------------------------------------------|
| **Manager**       | Full system access, manage all users & data       |
| **PA/Admin**      | Operational access, limited user management       |
| **Graphic Designer** | Job cards, portfolio, limited operational views |

---

## CI / Quality

GitHub Actions workflows are included:

- **ci-build-test.yml** – Restore → Build → Test (with coverage)
- **code-quality.yml** – Treat warnings as errors + vulnerable package scan

Both workflows ignore the `nado-graphics-website` folder and the `feature/portfolio-website` branch.

---

## Technology Stack

- **.NET 10**
- **ASP.NET Core** (MVC + Web API)
- **Entity Framework Core 10** + SQL Server
- **JWT Bearer Authentication**
- **Cookie Authentication**
- **SignalR**
- **QuestPDF**
- **BCrypt.Net-Next**
- **MailKit**
- **Swagger / Swashbuckle**

---

## Contributing

1. Create a feature branch from `main`
2. Follow existing coding style and comments
3. Ensure the solution builds with zero warnings (see code-quality workflow)
4. Open a Pull Request

---

## Presentation Video

YouTube Link: Insert Link here
Youtube URLs:
// Unverified account: maximum 15 minutes per video
1. Portfolio Website YouTube Demo: https://youtu.be/3p0sxSJpWpI
2. Management System YouTube Demo: https://youtu.be/SgdxLigqML0
3. PowerPoint code presentation: https://youtu.be/aQgATDpqNwM and https://youtu.be/0Vt9G6KCFe0
---
## Team Member Meeting Attendance

100% presence for all members for all in person and online meetings

---
