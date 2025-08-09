CREATE TABLE IF NOT EXISTS Dealers(
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Dealers" PRIMARY KEY AUTOINCREMENT,
    "Name" VARCHAR(255) NOT NULL,
    "Description" Text NULL,
    "ConcurrencyStamp" Varchar(255) Null
);

CREATE TABLE IF NOT EXISTS Cars (
  "Id"        INTEGER NOT NULL CONSTRAINT "PK_Cars" PRIMARY KEY AUTOINCREMENT,
  "DealerId"  INTEGER NOT NULL,
  "Make"      TEXT    NOT NULL,
  "Model"     TEXT    NOT NULL,
  "Year"      INTEGER NOT NULL,
  "Stock"     INTEGER NOT NULL, 
  "ConcurrencyStamp" Varchar(255) Null,
  CONSTRAINT "FK_Cars_Dealers_DealerId" FOREIGN KEY ("DealerId") REFERENCES "Dealers"("Id")
);


CREATE TABLE IF NOT EXISTS "AspNetRoles" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_AspNetRoles" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NULL,
    "NormalizedName" TEXT NULL,
    "ConcurrencyStamp" TEXT NULL
);


CREATE TABLE IF NOT EXISTS "AspNetRoleClaims" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_AspNetRoleClaims" PRIMARY KEY AUTOINCREMENT,
    "RoleId" INTEGER NOT NULL,
    "ClaimType" TEXT NULL,
    "ClaimValue" TEXT NULL,
    CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE
);


CREATE TABLE IF NOT EXISTS "AspNetUsers" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_AspNetUsers" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "Phone" TEXT NOT NULL,
    "DealerId" INTEGER NULL,
    "UserName" TEXT NULL,
    "NormalizedUserName" TEXT NULL,
    "Email" TEXT NULL,
    "NormalizedEmail" TEXT NULL,
    "EmailConfirmed" INTEGER NOT NULL,
    "PasswordHash" TEXT NULL,
    "SecurityStamp" TEXT NULL,
    "ConcurrencyStamp" TEXT NULL,
    "PhoneNumber" TEXT NULL,
    "PhoneNumberConfirmed" INTEGER NOT NULL,
    "TwoFactorEnabled" INTEGER NOT NULL,
    "LockoutEnd" TEXT NULL,
    "LockoutEnabled" INTEGER NOT NULL,
    "AccessFailedCount" INTEGER NOT NULL,
    CONSTRAINT "FK_AspNetUsers_Dealers_DealerId" FOREIGN KEY ("DealerId") REFERENCES "Dealers" ("Id")

);


CREATE TABLE IF NOT EXISTS "AspNetUserClaims" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_AspNetUserClaims" PRIMARY KEY AUTOINCREMENT,
    "UserId" INTEGER NOT NULL,
    "ClaimType" TEXT NULL,
    "ClaimValue" TEXT NULL,
    CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);


CREATE TABLE IF NOT EXISTS "AspNetUserLogins" (
    "LoginProvider" TEXT NOT NULL,
    "ProviderKey" TEXT NOT NULL,
    "ProviderDisplayName" TEXT NULL,
    "UserId" INTEGER NOT NULL,
    CONSTRAINT "PK_AspNetUserLogins" PRIMARY KEY ("LoginProvider", "ProviderKey"),
    CONSTRAINT "FK_AspNetUserLogins_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS "AspNetUserRoles" (
    "UserId" INTEGER NOT NULL,
    "RoleId" INTEGER NOT NULL,
    CONSTRAINT "PK_AspNetUserRoles" PRIMARY KEY ("UserId", "RoleId"),
    CONSTRAINT "FK_AspNetUserRoles_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_AspNetUserRoles_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);


CREATE TABLE IF NOT EXISTS "AspNetUserTokens" (
    "UserId" INTEGER NOT NULL,
    "LoginProvider" TEXT NOT NULL,
    "Name" TEXT NOT NULL,
    "Value" TEXT NULL,
    CONSTRAINT "PK_AspNetUserTokens" PRIMARY KEY ("UserId", "LoginProvider", "Name"),
    CONSTRAINT "FK_AspNetUserTokens_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

-- Indexes
CREATE INDEX IF NOT EXISTS "IX_AspNetRoleClaims_RoleId" ON "AspNetRoleClaims" ("RoleId");
CREATE INDEX IF NOT EXISTS "IX_AspNetUserClaims_UserId" ON "AspNetUserClaims" ("UserId");
CREATE INDEX IF NOT EXISTS "IX_AspNetUserLogins_UserId" ON "AspNetUserLogins" ("UserId");
CREATE INDEX IF NOT EXISTS "IX_AspNetUserRoles_RoleId" ON "AspNetUserRoles" ("RoleId");
CREATE INDEX IF NOT EXISTS "IX_AspNetUsers_DealerId" ON "AspNetUsers" ("DealerId");


-- Unique Constraints
CREATE UNIQUE INDEX IF NOT EXISTS "NonNormalizedRoleNameIndex" ON "AspNetRoles" ("Name");
CREATE UNIQUE INDEX IF NOT EXISTS "RoleNameIndex" ON "AspNetRoles" ("NormalizedName");
CREATE UNIQUE INDEX IF NOT EXISTS "EmailIndex" ON "AspNetUsers" ("NormalizedEmail");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_AspNetUsers_Email" ON "AspNetUsers" ("Email");
CREATE UNIQUE INDEX IF NOT EXISTS "UserNameIndex" ON "AspNetUsers" ("NormalizedUserName");
