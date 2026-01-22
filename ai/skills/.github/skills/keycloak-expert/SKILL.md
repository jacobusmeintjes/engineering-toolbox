---
name: keycloak-expert
description: Expert guidance for Keycloak identity and access management including realm configuration, OAuth2/OIDC, user federation, role-based access control, .NET integration, social login, and production deployment best practices
---

# Keycloak Expert

## When to use this skill

Use this skill when:
- Setting up Keycloak for authentication and authorization
- Configuring realms, clients, and users
- Implementing OAuth2/OIDC flows
- Integrating Keycloak with .NET applications
- Setting up user federation (LDAP, Active Directory)
- Implementing role-based access control (RBAC)
- Configuring social login providers
- Securing APIs with Keycloak
- Customizing themes and login flows

## Keycloak Fundamentals

### Core Concepts

**Realm** - Isolated namespace for users, clients, roles  
**Client** - Application that can request authentication  
**User** - Individual with login credentials  
**Role** - Permission assigned to users  
**Group** - Collection of users with common roles  
**Identity Provider** - External authentication source  
**User Federation** - Connect to LDAP/Active Directory

### Realm Configuration

```json
// Realm Settings (Admin Console)
{
  "realm": "myapp",
  "enabled": true,
  "displayName": "My Application",
  "displayNameHtml": "<b>My App</b>",
  
  // Login settings
  "registrationAllowed": true,
  "registrationEmailAsUsername": true,
  "resetPasswordAllowed": true,
  "rememberMe": true,
  "verifyEmail": true,
  "loginWithEmailAllowed": true,
  "duplicateEmailsAllowed": false,
  
  // Security settings
  "bruteForceProtected": true,
  "permanentLockout": false,
  "maxFailureWaitSeconds": 900,
  "minimumQuickLoginWaitSeconds": 60,
  "waitIncrementSeconds": 60,
  "quickLoginCheckMilliSeconds": 1000,
  "maxDeltaTimeSeconds": 43200,
  "failureFactor": 30,
  
  // Token settings
  "accessTokenLifespan": 300,
  "accessTokenLifespanForImplicitFlow": 900,
  "ssoSessionIdleTimeout": 1800,
  "ssoSessionMaxLifespan": 36000,
  "offlineSessionIdleTimeout": 2592000,
  "accessCodeLifespan": 60,
  "accessCodeLifespanUserAction": 300,
  "accessCodeLifespanLogin": 1800,
  
  // Required actions
  "requiredActions": [
    {
      "alias": "VERIFY_EMAIL",
      "name": "Verify Email",
      "enabled": true,
      "defaultAction": true
    },
    {
      "alias": "UPDATE_PASSWORD",
      "name": "Update Password",
      "enabled": true
    }
  ]
}
```

## Client Configuration

### Public Client (SPA/Blazor WASM)

```json
{
  "clientId": "myapp-frontend",
  "name": "My App Frontend",
  "description": "Blazor WebAssembly application",
  "enabled": true,
  "clientAuthenticatorType": "client-secret",
  "redirectUris": [
    "https://localhost:5001/*",
    "https://myapp.com/*"
  ],
  "webOrigins": [
    "https://localhost:5001",
    "https://myapp.com"
  ],
  "publicClient": true,
  "protocol": "openid-connect",
  "standardFlowEnabled": true,
  "implicitFlowEnabled": false,
  "directAccessGrantsEnabled": false,
  "serviceAccountsEnabled": false,
  "authorizationServicesEnabled": false,
  "fullScopeAllowed": false,
  
  // PKCE required for public clients
  "attributes": {
    "pkce.code.challenge.method": "S256"
  },
  
  // Optional claims
  "protocolMappers": [
    {
      "name": "roles",
      "protocol": "openid-connect",
      "protocolMapper": "oidc-usermodel-realm-role-mapper",
      "config": {
        "claim.name": "roles",
        "jsonType.label": "String",
        "id.token.claim": "true",
        "access.token.claim": "true",
        "userinfo.token.claim": "true"
      }
    }
  ]
}
```

### Confidential Client (Backend API)

```json
{
  "clientId": "myapp-api",
  "name": "My App API",
  "enabled": true,
  "clientAuthenticatorType": "client-secret",
  "secret": "********************",
  "redirectUris": [
    "https://api.myapp.com/*"
  ],
  "publicClient": false,
  "protocol": "openid-connect",
  "standardFlowEnabled": true,
  "directAccessGrantsEnabled": true,
  "serviceAccountsEnabled": true,
  "authorizationServicesEnabled": true,
  "bearerOnly": false,
  
  // Service account roles
  "serviceAccountRealmRoles": [
    "view-users",
    "manage-users"
  ],
  
  // Audience mapper
  "protocolMappers": [
    {
      "name": "audience",
      "protocol": "openid-connect",
      "protocolMapper": "oidc-audience-mapper",
      "config": {
        "included.client.audience": "myapp-api",
        "id.token.claim": "false",
        "access.token.claim": "true"
      }
    }
  ]
}
```

### Bearer-Only Client (API without frontend)

```json
{
  "clientId": "myapp-backend-api",
  "name": "Backend API",
  "enabled": true,
  "publicClient": false,
  "bearerOnly": true,
  "protocol": "openid-connect",
  "standardFlowEnabled": false,
  "directAccessGrantsEnabled": false,
  "serviceAccountsEnabled": false,
  "authorizationServicesEnabled": true
}
```

## .NET Integration

### ASP.NET Core Configuration

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add Keycloak authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    // Keycloak settings
    options.Authority = "https://keycloak.myapp.com/realms/myapp";
    options.ClientId = "myapp-frontend";
    options.ClientSecret = "client-secret-here";
    options.ResponseType = "code";
    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;
    options.RequireHttpsMetadata = true;
    
    // Scopes
    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
    options.Scope.Add("roles");
    
    // Map roles from token
    options.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = "preferred_username",
        RoleClaimType = "roles",
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true
    };
    
    // Events
    options.Events = new OpenIdConnectEvents
    {
        OnRedirectToIdentityProvider = context =>
        {
            // Add custom parameters
            context.ProtocolMessage.SetParameter("ui_locales", "en");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            // Process claims after token validation
            var identity = context.Principal?.Identity as ClaimsIdentity;
            
            // Add custom claims
            if (identity != null)
            {
                var emailClaim = identity.FindFirst("email");
                if (emailClaim != null)
                {
                    identity.AddClaim(new Claim(ClaimTypes.Email, emailClaim.Value));
                }
            }
            
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            context.Response.Redirect("/error");
            context.HandleResponse();
            return Task.CompletedTask;
        }
    };
});

// Authorization policies
builder.Services.AddAuthorizationCore(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("admin"));
    
    options.AddPolicy("CanViewCustomers", policy =>
        policy.RequireRole("customer-viewer", "admin"));
    
    options.AddPolicy("CanEditCustomers", policy =>
        policy.RequireRole("customer-editor", "admin"));
    
    options.AddPolicy("RequireEmailVerified", policy =>
        policy.RequireClaim("email_verified", "true"));
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
```

### Blazor Server Integration

```csharp
// Program.cs
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        options.Authority = "https://keycloak.myapp.com/realms/myapp";
        options.ClientId = "myapp-blazor";
        options.ClientSecret = "client-secret";
        options.ResponseType = "code";
        options.SaveTokens = true;
        options.GetClaimsFromUserInfoEndpoint = true;
        
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");
        options.Scope.Add("roles");
        
        options.TokenValidationParameters.RoleClaimType = "roles";
    });
```

```razor
@* _Host.cshtml or App.razor *@
<CascadingAuthenticationState>
    <Router AppAssembly="@typeof(App).Assembly">
        <Found Context="routeData">
            <AuthorizeRouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)">
                <NotAuthorized>
                    <h1>Not Authorized</h1>
                    <p>You are not authorized to view this page.</p>
                </NotAuthorized>
            </AuthorizeRouteView>
        </Found>
    </Router>
</CascadingAuthenticationState>
```

### Blazor WebAssembly Integration

```csharp
// Program.cs
var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddOidcAuthentication(options =>
{
    builder.Configuration.Bind("Keycloak", options.ProviderOptions);
    
    options.ProviderOptions.Authority = "https://keycloak.myapp.com/realms/myapp";
    options.ProviderOptions.ClientId = "myapp-wasm";
    options.ProviderOptions.ResponseType = "code";
    
    options.ProviderOptions.DefaultScopes.Add("openid");
    options.ProviderOptions.DefaultScopes.Add("profile");
    options.ProviderOptions.DefaultScopes.Add("email");
    options.ProviderOptions.DefaultScopes.Add("roles");
});

await builder.Build().RunAsync();
```

```json
// appsettings.json
{
  "Keycloak": {
    "Authority": "https://keycloak.myapp.com/realms/myapp",
    "ClientId": "myapp-wasm",
    "PostLogoutRedirectUri": "/"
  }
}
```

```razor
@* App.razor *@
<CascadingAuthenticationState>
    <Router AppAssembly="@typeof(App).Assembly">
        <Found Context="routeData">
            <AuthorizeRouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)">
                <NotAuthorized>
                    @if (context.User.Identity?.IsAuthenticated == true)
                    {
                        <p>You don't have permission to view this page.</p>
                    }
                    else
                    {
                        <RedirectToLogin />
                    }
                </NotAuthorized>
            </AuthorizeRouteView>
        </Found>
    </Router>
</CascadingAuthenticationState>
```

### API Protection

```csharp
// Program.cs - API
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://keycloak.myapp.com/realms/myapp";
        options.Audience = "myapp-api";
        options.RequireHttpsMetadata = true;
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            RoleClaimType = "roles",
            NameClaimType = "preferred_username"
        };
        
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                {
                    context.Response.Headers.Add("Token-Expired", "true");
                }
                return Task.CompletedTask;
            }
        };
    });

// Controller
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "customer-viewer")]
    public async Task<ActionResult<List<CustomerDto>>> GetCustomers()
    {
        var username = User.Identity?.Name;
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value);
        
        // Logic here
        return Ok(customers);
    }
    
    [HttpPost]
    [Authorize(Roles = "customer-editor")]
    public async Task<ActionResult<CustomerDto>> CreateCustomer(CreateCustomerRequest request)
    {
        // Logic here
        return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customer);
    }
}
```

## User Management

### Create User Programmatically

```csharp
public class KeycloakUserService
{
    private readonly HttpClient _httpClient;
    private readonly string _realm;
    
    public KeycloakUserService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _realm = configuration["Keycloak:Realm"]!;
        
        // Get admin token
        var token = GetAdminToken();
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
    }
    
    public async Task<string> CreateUserAsync(CreateUserRequest request)
    {
        var user = new
        {
            username = request.Email,
            email = request.Email,
            enabled = true,
            emailVerified = false,
            firstName = request.FirstName,
            lastName = request.LastName,
            credentials = new[]
            {
                new
                {
                    type = "password",
                    value = request.Password,
                    temporary = false
                }
            },
            requiredActions = new[] { "VERIFY_EMAIL" }
        };
        
        var response = await _httpClient.PostAsJsonAsync(
            $"/admin/realms/{_realm}/users",
            user);
        
        response.EnsureSuccessStatusCode();
        
        // Get user ID from Location header
        var location = response.Headers.Location!.ToString();
        var userId = location.Substring(location.LastIndexOf('/') + 1);
        
        return userId;
    }
    
    public async Task AssignRoleToUserAsync(string userId, string roleName)
    {
        // Get role representation
        var role = await GetRoleAsync(roleName);
        
        // Assign role to user
        await _httpClient.PostAsJsonAsync(
            $"/admin/realms/{_realm}/users/{userId}/role-mappings/realm",
            new[] { role });
    }
    
    private async Task<RoleRepresentation> GetRoleAsync(string roleName)
    {
        var response = await _httpClient.GetAsync(
            $"/admin/realms/{_realm}/roles/{roleName}");
        
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<RoleRepresentation>();
    }
}
```

## Role-Based Access Control (RBAC)

### Realm Roles

```json
// Realm roles configuration
{
  "roles": {
    "realm": [
      {
        "name": "admin",
        "description": "Administrator role with full access",
        "composite": true,
        "composites": {
          "realm": ["customer-manager", "order-manager", "user-manager"]
        }
      },
      {
        "name": "customer-viewer",
        "description": "Can view customer information"
      },
      {
        "name": "customer-editor",
        "description": "Can create and edit customers",
        "composite": true,
        "composites": {
          "realm": ["customer-viewer"]
        }
      },
      {
        "name": "customer-manager",
        "description": "Full customer management including delete",
        "composite": true,
        "composites": {
          "realm": ["customer-editor"]
        }
      }
    ]
  }
}
```

### Client Roles

```json
// Client-specific roles
{
  "clients": [
    {
      "clientId": "myapp-api",
      "roles": [
        {
          "name": "api:read",
          "description": "Read access to API"
        },
        {
          "name": "api:write",
          "description": "Write access to API"
        },
        {
          "name": "api:admin",
          "description": "Admin access to API",
          "composite": true,
          "composites": {
            "client": {
              "myapp-api": ["api:read", "api:write"]
            }
          }
        }
      ]
    }
  ]
}
```

### Role Mapping in .NET

```csharp
// Custom authorization handler
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var roles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value);
        
        if (roles.Any(r => requirement.AllowedRoles.Contains(r)))
        {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}

public class PermissionRequirement : IAuthorizationRequirement
{
    public string[] AllowedRoles { get; }
    
    public PermissionRequirement(params string[] allowedRoles)
    {
        AllowedRoles = allowedRoles;
    }
}

// Registration
builder.Services.AddAuthorizationCore(options =>
{
    options.AddPolicy("CanEditCustomers", policy =>
        policy.Requirements.Add(new PermissionRequirement("customer-editor", "admin")));
});

builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
```

## User Federation

### LDAP Configuration

```json
{
  "id": "ldap-federation",
  "name": "ldap-users",
  "providerId": "ldap",
  "providerType": "org.keycloak.storage.UserStorageProvider",
  "config": {
    "enabled": ["true"],
    "priority": ["1"],
    "editMode": ["READ_ONLY"],
    "syncRegistrations": ["false"],
    "vendor": ["ad"],
    "connectionUrl": ["ldap://ldap.mycompany.com:389"],
    "bindDn": ["cn=admin,dc=mycompany,dc=com"],
    "bindCredential": ["password"],
    "usersDn": ["ou=users,dc=mycompany,dc=com"],
    "authType": ["simple"],
    "searchScope": ["2"],
    "useTruststoreSpi": ["ldapsOnly"],
    "connectionPooling": ["true"],
    "pagination": ["true"],
    "usernameLDAPAttribute": ["uid"],
    "rdnLDAPAttribute": ["uid"],
    "uuidLDAPAttribute": ["entryUUID"],
    "userObjectClasses": ["inetOrgPerson, organizationalPerson"],
    "fullSyncPeriod": ["604800"],
    "changedSyncPeriod": ["86400"],
    "cachePolicy": ["DEFAULT"]
  }
}
```

### Active Directory Integration

```json
{
  "providerId": "ldap",
  "config": {
    "vendor": ["ad"],
    "connectionUrl": ["ldap://dc.mycompany.com:389"],
    "bindDn": ["CN=Service Account,OU=Service Accounts,DC=mycompany,DC=com"],
    "bindCredential": ["password"],
    "usersDn": ["OU=Users,DC=mycompany,DC=com"],
    "authType": ["simple"],
    "searchScope": ["2"],
    "usernameLDAPAttribute": ["sAMAccountName"],
    "rdnLDAPAttribute": ["cn"],
    "uuidLDAPAttribute": ["objectGUID"],
    "userObjectClasses": ["person, organizationalPerson, user"],
    "customUserSearchFilter": ["(&(objectClass=user)(!(userAccountControl:1.2.840.113556.1.4.803:=2)))"]
  }
}
```

## Social Login

### Google Configuration

```json
{
  "identityProviders": [
    {
      "alias": "google",
      "providerId": "google",
      "enabled": true,
      "updateProfileFirstLoginMode": "on",
      "trustEmail": true,
      "storeToken": false,
      "addReadTokenRoleOnCreate": false,
      "authenticateByDefault": false,
      "linkOnly": false,
      "firstBrokerLoginFlowAlias": "first broker login",
      "config": {
        "clientId": "google-client-id.apps.googleusercontent.com",
        "clientSecret": "google-client-secret",
        "defaultScope": "openid profile email",
        "guiOrder": "1"
      }
    }
  ]
}
```

### Microsoft/Azure AD

```json
{
  "alias": "microsoft",
  "providerId": "microsoft",
  "enabled": true,
  "config": {
    "clientId": "azure-app-client-id",
    "clientSecret": "azure-app-client-secret",
    "defaultScope": "openid profile email",
    "tenant": "common"
  }
}
```

### GitHub

```json
{
  "alias": "github",
  "providerId": "github",
  "enabled": true,
  "config": {
    "clientId": "github-oauth-app-client-id",
    "clientSecret": "github-oauth-app-client-secret",
    "defaultScope": "user:email"
  }
}
```

## Custom Themes

### Directory Structure

```
themes/
└── myapp-theme/
    ├── login/
    │   ├── theme.properties
    │   ├── messages/
    │   │   ├── messages_en.properties
    │   │   └── messages_es.properties
    │   ├── resources/
    │   │   ├── css/
    │   │   │   └── login.css
    │   │   ├── img/
    │   │   │   └── logo.png
    │   │   └── js/
    │   │       └── login.js
    │   └── login.ftl
    ├── account/
    │   └── theme.properties
    ├── admin/
    │   └── theme.properties
    └── email/
        └── theme.properties
```

### Custom Login Page

```html
<!-- login.ftl -->
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>${msg("loginTitle", realm.displayName)}</title>
    <link rel="stylesheet" href="${url.resourcesPath}/css/login.css">
</head>
<body>
    <div class="login-container">
        <div class="login-card">
            <div class="login-header">
                <img src="${url.resourcesPath}/img/logo.png" alt="Logo">
                <h1>${msg("loginTitle")}</h1>
            </div>
            
            <#if message?has_content>
                <div class="alert alert-${message.type}">
                    ${kcSanitize(message.summary)?no_esc}
                </div>
            </#if>
            
            <form id="kc-form-login" onsubmit="login.disabled = true; return true;" 
                  action="${url.loginAction}" method="post">
                
                <div class="form-group">
                    <label for="username">${msg("usernameOrEmail")}</label>
                    <input 
                        type="text" 
                        id="username" 
                        name="username" 
                        value="${(login.username!'')}" 
                        autofocus 
                        autocomplete="off"
                        required>
                </div>
                
                <div class="form-group">
                    <label for="password">${msg("password")}</label>
                    <input 
                        type="password" 
                        id="password" 
                        name="password" 
                        autocomplete="off"
                        required>
                </div>
                
                <#if realm.rememberMe && !usernameEditDisabled??>
                    <div class="form-group">
                        <input 
                            type="checkbox" 
                            id="rememberMe" 
                            name="rememberMe"
                            <#if login.rememberMe??>checked</#if>>
                        <label for="rememberMe">${msg("rememberMe")}</label>
                    </div>
                </#if>
                
                <button type="submit" class="btn btn-primary">
                    ${msg("doLogIn")}
                </button>
            </form>
            
            <#if realm.password && realm.resetPasswordAllowed>
                <div class="login-footer">
                    <a href="${url.loginResetCredentialsUrl}">
                        ${msg("doForgotPassword")}
                    </a>
                </div>
            </#if>
            
            <#if realm.password && social.providers??>
                <div class="social-login">
                    <p>${msg("identity-provider-login-label")}</p>
                    <#list social.providers as p>
                        <a href="${p.loginUrl}" class="social-button ${p.alias}">
                            <span>${p.displayName}</span>
                        </a>
                    </#list>
                </div>
            </#if>
        </div>
    </div>
</body>
</html>
```

## Production Deployment

### Docker Compose

```yaml
version: '3.8'

services:
  postgres:
    image: postgres:15
    environment:
      POSTGRES_DB: keycloak
      POSTGRES_USER: keycloak
      POSTGRES_PASSWORD: password
    volumes:
      - postgres-data:/var/lib/postgresql/data
    networks:
      - keycloak-network
    restart: unless-stopped

  keycloak:
    image: quay.io/keycloak/keycloak:23.0
    command: start --optimized
    environment:
      KC_DB: postgres
      KC_DB_URL: jdbc:postgresql://postgres:5432/keycloak
      KC_DB_USERNAME: keycloak
      KC_DB_PASSWORD: password
      KC_HOSTNAME: keycloak.myapp.com
      KC_HOSTNAME_STRICT: true
      KC_HOSTNAME_STRICT_HTTPS: true
      KC_PROXY: edge
      KC_HTTP_ENABLED: true
      KEYCLOAK_ADMIN: admin
      KEYCLOAK_ADMIN_PASSWORD: admin-password-change-me
      KC_LOG_LEVEL: INFO
      KC_METRICS_ENABLED: true
      KC_HEALTH_ENABLED: true
    ports:
      - "8080:8080"
    depends_on:
      - postgres
    networks:
      - keycloak-network
    restart: unless-stopped
    volumes:
      - ./themes:/opt/keycloak/themes
    healthcheck:
      test: ["CMD-SHELL", "exec 3<>/dev/tcp/127.0.0.1/8080;echo -e 'GET /health/ready HTTP/1.1\r\nhost: 127.0.0.1:8080\r\nConnection: close\r\n\r\n' >&3;if [ $? -eq 0 ]; then echo 'Healthcheck Successful';exit 0;else echo 'Healthcheck Failed';exit 1;fi;"]
      interval: 30s
      timeout: 10s
      retries: 3

  nginx:
    image: nginx:alpine
    ports:
      - "443:443"
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf
      - ./ssl:/etc/nginx/ssl
    depends_on:
      - keycloak
    networks:
      - keycloak-network
    restart: unless-stopped

volumes:
  postgres-data:

networks:
  keycloak-network:
    driver: bridge
```

### Nginx Reverse Proxy

```nginx
upstream keycloak {
    server keycloak:8080;
}

server {
    listen 443 ssl http2;
    server_name keycloak.myapp.com;

    ssl_certificate /etc/nginx/ssl/cert.pem;
    ssl_certificate_key /etc/nginx/ssl/key.pem;
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;

    location / {
        proxy_pass http://keycloak;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_set_header X-Forwarded-Port $server_port;
        
        proxy_buffer_size 128k;
        proxy_buffers 4 256k;
        proxy_busy_buffers_size 256k;
    }
}

server {
    listen 80;
    server_name keycloak.myapp.com;
    return 301 https://$server_name$request_uri;
}
```

### High Availability Setup

```yaml
# Multiple Keycloak instances behind load balancer
services:
  keycloak-1:
    image: quay.io/keycloak/keycloak:23.0
    # ... configuration
    
  keycloak-2:
    image: quay.io/keycloak/keycloak:23.0
    # ... configuration
    
  keycloak-3:
    image: quay.io/keycloak/keycloak:23.0
    # ... configuration
    
  haproxy:
    image: haproxy:2.8
    volumes:
      - ./haproxy.cfg:/usr/local/etc/haproxy/haproxy.cfg
    ports:
      - "443:443"
    depends_on:
      - keycloak-1
      - keycloak-2
      - keycloak-3
```

## Security Best Practices

### Token Configuration

```
Access Token Lifespan: 5 minutes (300 seconds)
Refresh Token Lifespan: 30 minutes (1800 seconds)
SSO Session Idle: 30 minutes
SSO Session Max: 10 hours
Offline Session Idle: 30 days

For APIs:
- Short access token lifetime (1-5 min)
- No refresh tokens
- Use client credentials for service-to-service
```

### Security Headers

```csharp
// Add security headers in .NET
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Add(
        "Content-Security-Policy",
        "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline';");
    
    await next();
});
```

### Rate Limiting

```json
// Keycloak brute force protection
{
  "bruteForceProtected": true,
  "permanentLockout": false,
  "maxFailureWaitSeconds": 900,
  "minimumQuickLoginWaitSeconds": 60,
  "waitIncrementSeconds": 60,
  "quickLoginCheckMilliSeconds": 1000,
  "maxDeltaTimeSeconds": 43200,
  "failureFactor": 5
}
```

## Monitoring and Logging

### Health Checks

```bash
# Health endpoint
curl https://keycloak.myapp.com/health

# Readiness
curl https://keycloak.myapp.com/health/ready

# Liveness
curl https://keycloak.myapp.com/health/live
```

### Metrics

```bash
# Prometheus metrics
curl https://keycloak.myapp.com/metrics
```

### Logging Configuration

```bash
# Environment variables
KC_LOG_LEVEL=info
KC_LOG_CONSOLE_OUTPUT=json
KC_LOG_CONSOLE_COLOR=false
```

## Backup and Restore

### Database Backup

```bash
# Backup PostgreSQL
docker exec keycloak-postgres pg_dump -U keycloak keycloak > keycloak-backup.sql

# Restore
docker exec -i keycloak-postgres psql -U keycloak keycloak < keycloak-backup.sql
```

### Realm Export/Import

```bash
# Export realm
docker exec keycloak /opt/keycloak/bin/kc.sh export \
  --dir /tmp/export \
  --realm myapp

# Import realm
docker exec keycloak /opt/keycloak/bin/kc.sh import \
  --dir /tmp/import \
  --realm myapp
```

## Troubleshooting

### Common Issues

**Issue: "Invalid redirect URI"**
```
Solution: Ensure redirect URI in client config matches exactly,
including protocol (https://), trailing slashes
```

**Issue: "Role not appearing in token"**
```
Solution: Check client scope mappers, ensure role mapper
is configured with correct token claim name
```

**Issue: "CORS errors"**
```
Solution: Add Web Origins in client configuration
```

**Issue: "Token expired too quickly"**
```
Solution: Adjust access token lifespan in realm settings
```

## Resources

- [Keycloak Documentation](https://www.keycloak.org/documentation)
- [Keycloak Admin REST API](https://www.keycloak.org/docs-api/latest/rest-api/)
- [Server Administration Guide](https://www.keycloak.org/docs/latest/server_admin/)
- [Securing Applications](https://www.keycloak.org/docs/latest/securing_apps/)
