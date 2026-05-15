# QuestProject — E-commerce (Angular + ASP.NET Core + SQL Server)

Vertical slice: products, cart (RxJS), authenticated checkout, orders persisted in SQL Server. **No ORM** — data access uses ADO.NET (`SqlConnection` / `SqlCommand`).

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (project targets **net10.0**)
- [Node.js](https://nodejs.org/) + npm
- SQL Server (Express or full) + optional SSMS

## 1. Database

1. Create database and objects using the script in this repo:

   - File: `database/schema.sql`
   - In SSMS: open the file, set paths if needed, execute against your server.

2. **Already have `QuestDb`?** Reload the refurbished PC catalog (clears orders + products, then inserts **38 products**):

   - File: `database/seed-pc-components.sql`

3. Update the connection string in `backend/appsettings.json`:

   - Key: `ConnectionStrings:DefaultConnection`
   - Example: `Server=localhost\\SQLEXPRESS;Database=QuestDb;Trusted_Connection=True;TrustServerCertificate=True;`

4. **Submission note:** if the exercise asks for a `.bak`, export a backup from SSMS after the DB is populated (`Tasks` → `Back up database…`) and add the file to the repo or release assets.

## 2. Backend (API)

```bash
cd backend
dotnet restore
dotnet run
```

- Default URL (see `Properties/launchSettings.json`): **http://localhost:5261**
- Swagger UI is enabled while developing.

### API overview

| Method | Route | Auth | Description |
|--------|--------|------|-------------|
| GET | `/api/products` | No | List products |
| POST | `/api/auth/register` | No | Register user |
| POST | `/api/auth/login` | No | Login → returns JWT |
| POST | `/api/checkout` | No | Legacy demo checkout (recalculates totals from DB) |
| POST | `/api/orders` | **Bearer JWT** | Create order + order lines (totals from `Products`) |
| GET | `/api/orders/my` | **Bearer JWT** | Current user’s orders |

JWT settings: `backend/appsettings.json` → `Jwt` section (`Key`, `Issuer`, `Audience`, `ExpiresMinutes`).

### Unit tests (backend)

```bash
dotnet test backend.Tests/backend.Tests.csproj
```

## 3. Frontend (Angular)

```bash
cd frontend
npm install
npm start
```

- App: **http://localhost:4200**
- API base URL is currently hardcoded to `http://localhost:5261` in services (adjust if your API port differs).

### Routes

| Path | Description |
|------|-------------|
| `/` | Product list + add to cart |
| `/cart` | Cart + checkout (shipping, place order) |
| `/login`, `/register` | Auth |
| `/orders` | My orders (requires login) |

### Unit tests (frontend)

```bash
cd frontend
npm test
```

Uses Angular’s **Vitest** builder (`@angular/build:unit-test`).

## 4. Architecture notes

- **Cart state:** `CartService` with `BehaviorSubject` for cart lines; components subscribe to `cartItems$`.
- **Auth state:** `AuthService` with persisted session (`localStorage`) + JWT attached via HTTP interceptor.
- **Checkout:** `POST /api/orders` builds line totals from **database product prices**; the frontend total is display-only.
- **Passwords:** target schema uses `Users.PasswordHash` (SHA-256 hex). The API can still work with a legacy `Password` column if present (for migration).

## 5. Troubleshooting

- **401 on `/api/orders`:** log in again; token may be expired.
- **SQL “Invalid column name …”:** align your database with `database/schema.sql`, or adjust column names in `OrdersController` / `AuthController` to match your instance.

## License

Exercise / demo project.
