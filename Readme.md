# Car Stock API

A simple ASP.NET Core Web API for managing car inventory (“stock”) by dealers, with **SuperAdmin** & **Dealer** roles, JWT authentication

---

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download) (or matching your TargetFramework)
- Git client
- (Optional for SQL mode) SQL Server instance or Docker image

---

## Running Steps

1. **Clone the repository into api folder**

```bash
git clone https://github.com/ChaudharySabin/OracleCMSCarStockApi.git api
cd api/
```

2. **Initialize & configure user-secrets**

```bash
dotnet user-secrets init
dotnet user-secrets set "JWT:SigningKey"  "a-very-very-very-very-very-very-very-very-very-very-very-very-very-very-very-very-very-very-very-very-very-very-very-very-very-long-secret-string"
dotnet user-secrets set "JWT:Issuer"      "CarStockApp"
dotnet user-secrets set "JWT:Audience"    "CarStockClients"
dotnet user-secrets set "JWT:ExpiryMinutes"    30
```

3. **Configure appsetting.json to include UseInMemoryDatabase:true**

```json
{
  "UseInMemoryDatabase": true
}
```

4. **Run the API**

```bash
dotnet restore
dotnet run
```

- The In-Memory database is auto-created & seeded with:
  - 10 dealers & one Dealer user each
  - 10 cars per dealer
  - 1 SuperAdmin user

5. **Browse the API**
   Visit http://localhost:5265/swagger (or the HTTPS URL shown).

### Default Credential

SuperAdminEmail: superadmin@example.com
Password: Password123#

DealerUserEmail: dealer@example.com
Password: Password123#

## Note

New Registeration will be automatically marked as `Dealer` role but will not be assigned to any dealer.
Only, SuperAdmin users have the authority to assign users into different dealer
