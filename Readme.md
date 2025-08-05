# Car Stock API

A simple ASP.NET Core Web API for managing car inventory (“stock”) by dealers, with **SuperAdmin** & **Dealer** roles, JWT authentication

---

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download) (or matching your TargetFramework)
- Git client
- (Optional for SQL mode) SQL Server instance

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

3. **Configure `appsetting.json` to include UseInMemoryDatabase:true**

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

## Optional

1. ** To use the reset password token feature, Please configure the localSMTP using SMTP4Dev**

- Install the tool

```bash
dotnet tool install --global Rnwood.Smtp4dev
```

- Provide secrets

```bash
dotnet user-secrets set "Smtp:Host"  "localhost"
dotnet user-secrets set "Smtp:Port"      25
dotnet user-secrets set "Smtp:From"    "noreply@carstock.local"
```

- Run the tool (You can also use the docker for SMTP4Dev)

```bash
smtp4dev
```

2. **To Use SQLServer, Configure `appsetting.json` and remove `UseInMemoryDatabase:false` and set user-secrets for connection string**

```json
{
  "UseInMemoryDatabase": false
}
```

```bash
dotnet user-secrets set "ConnectionStrings:SQLSERVERCONNECTION" = YourConnectionStringHere
```

2. **Create Migrations**

```bash
dotnet tool install -g dotnet-ef
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Docker

1. **Pull the docker image**

```bash
docker pull chaudharysabin/carstockapi:v1.2
```

2. **Run the docker container\***

```bash
docker run -d \
  --name carstock-api \
  -p <Port>:80 \
  -e UseInMemoryDatabase=true \
  -e Smtp__Host="host.docker.internal" \
  -e Smtp__Port="25" \
  -e Smtp__From="<YourFromEmail>" \
  -e JWT__SigningKey="<YourVeryLongSigningKey>" \
  -e JWT__Issuer="YourIssue" \
  -e JWT__Audience="YourAudence" \
  -e ASPNETCORE_ENVIRONMENT="development" \
  chaudharysabin/carstockapi:v1.2
```

-Example:

```bash
docker run -d \
  --name carstock-api \
  -p 5256:80 \
  -e UseInMemoryDatabase=true \
  -e Smtp__Host="host.docker.internal" \
  -e Smtp__Port="25" \
  -e Smtp__From="noreply@localhost" \
  -e JWT__SigningKey="MySuperSecretKey123!" \
  -e JWT__Issuer="CarStockApp" \
  -e JWT__Audience="CarStockClients" \
  -e ASPNETCORE_ENVIRONMENT="Development" \
  chaudharysabin/carstockapi:v1.2
```
