# Car Stock API

A simple ASP.NET Core Web API for managing car inventory (“stock”) by dealers, with **SuperAdmin** & **Dealer** roles, JWT authentication

---

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download) (or matching your TargetFramework)
- Git client

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

3. **Ensure `SQLITECONNECTION` is either set in environment variable or in `appsetting.json`**

```json
{
  "SQLITECONNECTION": "Data Source=Database/CarStock.db"
}
```

4. **Run the API**

```bash
dotnet restore
dotnet run
```

5. **Browse the API**
   Visit http://localhost:5265/swagger (or the HTTPS URL shown).

### Default Credential

SuperAdminEmail: superadmin@example.com
Password: Password123#

DealerUserEmail: dealer@example.com
Password: Password123#

## Note

New Registeration will be automatically marked as `Dealer` role but will not be assigned to any dealer.
Only, SuperAdmin users have the authority to assign users into different dealer.

The database will be seeded on startup if there is no any records.

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

## Docker

1. **Pull the docker image**

```bash
docker pull chaudharysabin/carstockapi:latest
```

**or**
**build the image from the cloned repo**

```bash
docker compose up --build
```

**(Note this will run your docker image so you can ignore the commands below)**

2. **Run the docker container if pulled from the dockerhub**

```bash
docker run -d \
  --name carstock-api \
  -p <Port>:80 \
  -e Smtp__Host="host.docker.internal" \
  -e Smtp__Port="25" \
  -e Smtp__From="<YourFromEmail>" \
  -e JWT__SigningKey="<YourVeryLongSigningKey>" \
  -e JWT__Issuer="YourIssue" \
  -e JWT__Audience="YourAudence" \
  -e SQLITECONNECTION="Data Source=Database/CarStock.db" \
  -e ASPNETCORE_ENVIRONMENT="development" \
  chaudharysabin/carstockapi:latest
```

-Example:

```bash
docker run -d \
  --name carstock-api \
  -p 5256:80 \
  -e Smtp__Host="host.docker.internal" \
  -e Smtp__Port="25" \
  -e Smtp__From="noreply@localhost" \
  -e JWT__SigningKey="MySuperSecretKey123456789456478841216514041084654894214065!" \
  -e JWT__Issuer="CarStockApp" \
  -e JWT__Audience="CarStockClients" \
  -e SQLITECONNECTION="Data Source=Database/CarStock.db" \
  -e ASPNETCORE_ENVIRONMENT="Development" \
  chaudharysabin/carstockapi:latest
```

**(Note: Please run Smpt4dev when running the docker image like this or you can run smtp4dev as a docker service)**
