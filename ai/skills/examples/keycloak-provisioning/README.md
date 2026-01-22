# Aspire + Keycloak (with code-based provisioning + Postgres persistence)

This example shows how to run Keycloak in a .NET Aspire AppHost with **Postgres-backed persistence** and provision a realm, client, and user **in code** using a one-shot "provisioner" project.

## What gets created

When the AppHost starts:

- A Postgres container is started (`keycloak-postgres`) with a persistent data volume.
- A Keycloak container is started (`keycloak`) configured to use Postgres.
- The `KeycloakProvisioningSample.KeycloakProvisioner` project waits for Keycloak readiness, then **idempotently** creates:
  - Realm: `demo`
  - Realm roles: `admin`, `manager`, `developer`, `viewer`
  - **4 tenants** (as separate clients):
    - `acme-corp` (Acme Corporation - Manufacturing & Retail)
    - `stellar-tech` (Stellar Technologies - Software & Cloud Services)
    - `global-logistics` (Global Logistics Inc - Supply Chain & Shipping)
    - `finwave` (FinWave Solutions - Financial Services & Banking)
  - **4 users per tenant** (16 users total), each with different roles:
    - `{tenant}.admin` with `admin` role (password: `admin`)
    - `{tenant}.manager` with `manager` role (password: `manager`)
    - `{tenant}.developer` with `developer` role (password: `developer`)
    - `{tenant}.viewer` with `viewer` role (password: `viewer`)

Each user has a `tenant` attribute matching their client/tenant for multitenancy scenarios.

## Persistence

Because Keycloak uses Postgres with a data volume, **realms/clients/users survive container restarts**. The provisioner is idempotent:
- It checks if the realm/client/user already exist before creating them.
- It resets the demo user's password on each run (so you always have a known working credential).

This means you can stop/restart the AppHost and your Keycloak configuration persists.

## Run it

From this folder:

- `dotnet run --project .\KeycloakProvisioningSample.AppHost`

Then open the Aspire dashboard to see endpoints/logs.

Keycloak admin credentials (dev default):

- Username: `admin`
- Password: `admin`

The admin password is set in `KeycloakProvisioningSample.AppHost/appsettings.Development.json` via the Aspire parameter `keycloak-admin-password`.

## Get a token and call the secured API

1) Get an access token from Keycloak (password grant) for any tenant/user:

Example for Acme Corp admin user:
- Token endpoint: `http://<keycloak-host>:<keycloak-port>/realms/demo/protocol/openid-connect/token`
- Form fields:
  - `grant_type=password`
  - `client_id=acme-corp` (or `stellar-tech`, `global-logistics`, `finwave`)
  - `username=acme-corp.admin` (or any other user like `acme-corp.developer`)
  - `password=admin` (matches the role: `admin`, `manager`, `developer`, or `viewer`)

Example using curl:
```bash
curl -X POST "http://localhost:<keycloak-port>/realms/demo/protocol/openid-connect/token" \
  -d "grant_type=password" \
  -d "client_id=acme-corp" \
  -d "username=acme-corp.admin" \
  -d "password=admin"
```

2) Call the API:

- `GET http://<api-host>:<api-port>/secure`
- Header: `Authorization: Bearer <access_token>`

The token will include the user's realm role and tenant attribute for authorization checks.

## Notes

- The provisioner is intentionally **idempotent** (it checks for existing realm/client/user and only creates if missing). It also resets the demo user's password on each run.
- **Postgres persistence**: Both Keycloak's database and Postgres data are persisted via Docker volumes. Stop/restart the AppHost and your realms/users remain.
- For a minimal sample, the API config sets `ValidateAudience=false`. In a real system, you should validate audience and configure Keycloak mappers accordingly.
- First startup takes longer (Keycloak must initialize its schema in Postgres). Subsequent starts are faster because the database already exists.
