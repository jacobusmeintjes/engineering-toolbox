using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

var keycloakUrl = GetRequired("KEYCLOAK_URL").TrimEnd('/');
var adminUsername = GetRequired("KEYCLOAK_ADMIN_USERNAME");
var adminPassword = GetRequired("KEYCLOAK_ADMIN_PASSWORD");
var realmName = GetRequired("KEYCLOAK_REALM");

Console.WriteLine($"Keycloak multitenant provisioner starting. BaseUrl={keycloakUrl}, Realm={realmName}");

using var http = new HttpClient
{
	BaseAddress = new Uri($"{keycloakUrl}/")
};

await WaitForKeycloakReadyAsync(http, TimeSpan.FromMinutes(3));

var adminToken = await GetAdminAccessTokenAsync(http, adminUsername, adminPassword);
http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

await EnsureRealmAsync(http, realmName);

// Define realm roles for authorization
var roles = new[] { "admin", "manager", "developer", "viewer" };
foreach (var role in roles)
{
	await EnsureRealmRoleAsync(http, realmName, role);
}

// Define 4 fictitious companies (tenants) - each will get its own client
var tenants = new[]
{
	new Tenant("acme-corp", "Acme Corporation", "Manufacturing & Retail"),
	new Tenant("stellar-tech", "Stellar Technologies", "Software & Cloud Services"),
	new Tenant("global-logistics", "Global Logistics Inc", "Supply Chain & Shipping"),
	new Tenant("finwave", "FinWave Solutions", "Financial Services & Banking")
};

// Create a client for each tenant
foreach (var tenant in tenants)
{
	await EnsureClientAsync(http, realmName, tenant.ClientId, tenant.Name);
}

// Create 4 users per tenant with different roles
foreach (var tenant in tenants)
{
	var users = new[]
	{
		new UserData($"{tenant.Prefix}.admin", "Admin", "User", $"admin@{tenant.Prefix}.example.com", "admin", "admin"),
		new UserData($"{tenant.Prefix}.manager", "Manager", "Smith", $"manager@{tenant.Prefix}.example.com", "manager", "manager"),
		new UserData($"{tenant.Prefix}.developer", "Dev", "Jones", $"dev@{tenant.Prefix}.example.com", "developer", "developer"),
		new UserData($"{tenant.Prefix}.viewer", "View", "Brown", $"viewer@{tenant.Prefix}.example.com", "viewer", "viewer")
	};

	foreach (var user in users)
	{
		await EnsureUserAsync(http, realmName, user, tenant.ClientId);
	}
}

Console.WriteLine("Keycloak multitenant provisioning complete.");
Console.WriteLine($"Created {tenants.Length} tenants with {tenants.Length * 4} users total.");

static string GetRequired(string name)
{
	var value = Environment.GetEnvironmentVariable(name);
	if (string.IsNullOrWhiteSpace(value))
	{
		throw new InvalidOperationException($"Missing required environment variable '{name}'.");
	}

	return value;
}

static async Task WaitForKeycloakReadyAsync(HttpClient http, TimeSpan timeout)
{
	using var cts = new CancellationTokenSource(timeout);

	while (true)
	{
		cts.Token.ThrowIfCancellationRequested();

		try
		{
			using var response = await http.GetAsync("health/ready", cts.Token);
			if (response.IsSuccessStatusCode)
			{
				Console.WriteLine("Keycloak is ready.");
				return;
			}
		}
		catch (HttpRequestException)
		{
			// ignore until ready
		}

		await Task.Delay(TimeSpan.FromSeconds(2), cts.Token);
	}
}

static async Task<string> GetAdminAccessTokenAsync(HttpClient http, string username, string password)
{
	using var tokenResponse = await http.PostAsync(
		"realms/master/protocol/openid-connect/token",
		new FormUrlEncodedContent(new Dictionary<string, string>
		{
			["grant_type"] = "password",
			["client_id"] = "admin-cli",
			["username"] = username,
			["password"] = password
		}));

	tokenResponse.EnsureSuccessStatusCode();

	using var stream = await tokenResponse.Content.ReadAsStreamAsync();
	using var json = await JsonDocument.ParseAsync(stream);
	var token = json.RootElement.GetProperty("access_token").GetString();
	if (string.IsNullOrWhiteSpace(token))
	{
		throw new InvalidOperationException("Keycloak token response did not include 'access_token'.");
	}

	return token;
}

static async Task EnsureRealmAsync(HttpClient http, string realmName)
{
	var exists = await ResourceExistsAsync(http, $"admin/realms/{Uri.EscapeDataString(realmName)}");
	if (exists)
	{
		Console.WriteLine($"Realm '{realmName}' already exists.");
		return;
	}

	Console.WriteLine($"Creating realm '{realmName}'...");

	using var response = await http.PostAsJsonAsync("admin/realms", new
	{
		realm = realmName,
		enabled = true
	});

	response.EnsureSuccessStatusCode();
}

static async Task EnsureRealmRoleAsync(HttpClient http, string realmName, string roleName)
{
	var existingRoles = await http.GetFromJsonAsync<List<RoleRepresentation>>(
		$"admin/realms/{Uri.EscapeDataString(realmName)}/roles");

	if (existingRoles?.Any(r => r.Name == roleName) == true)
	{
		Console.WriteLine($"Role '{roleName}' already exists.");
		return;
	}

	Console.WriteLine($"Creating realm role '{roleName}'...");

	using var response = await http.PostAsJsonAsync(
		$"admin/realms/{Uri.EscapeDataString(realmName)}/roles",
		new
		{
			name = roleName,
			description = $"{roleName} role"
		});

	response.EnsureSuccessStatusCode();
}

static async Task EnsureClientAsync(HttpClient http, string realmName, string clientId, string clientName)
{
	var existing = await http.GetFromJsonAsync<List<ClientRepresentation>>(
		$"admin/realms/{Uri.EscapeDataString(realmName)}/clients?clientId={Uri.EscapeDataString(clientId)}");

	if (existing is { Count: > 0 })
	{
		Console.WriteLine($"Client '{clientId}' already exists.");
		return;
	}

	Console.WriteLine($"Creating client '{clientId}' ({clientName})...");

	using var response = await http.PostAsJsonAsync(
		$"admin/realms/{Uri.EscapeDataString(realmName)}/clients",
		new
		{
			clientId,
			name = clientName,
			enabled = true,
			publicClient = true,
			standardFlowEnabled = true,
			directAccessGrantsEnabled = true,
			redirectUris = new[] { "http://localhost:*/*", "https://localhost:*/*" },
			webOrigins = new[] { "+" },
			attributes = new Dictionary<string, string>
			{
				["tenant"] = clientId
			}
		});

	response.EnsureSuccessStatusCode();
}

static async Task EnsureUserAsync(HttpClient http, string realmName, UserData userData, string tenantClientId)
{
	var users = await http.GetFromJsonAsync<List<UserRepresentation>>(
		$"admin/realms/{Uri.EscapeDataString(realmName)}/users?username={Uri.EscapeDataString(userData.Username)}&exact=true");

	string? userId;

	if (users is { Count: > 0 } && !string.IsNullOrWhiteSpace(users[0].Id))
	{
		userId = users[0].Id;
		Console.WriteLine($"User '{userData.Username}' already exists.");
	}
	else
	{
		Console.WriteLine($"Creating user '{userData.Username}' ({userData.FirstName} {userData.LastName}) for tenant '{tenantClientId}'...");

		using var create = await http.PostAsJsonAsync(
			$"admin/realms/{Uri.EscapeDataString(realmName)}/users",
			new
			{
				username = userData.Username,
				firstName = userData.FirstName,
				lastName = userData.LastName,
				email = userData.Email,
				enabled = true,
				emailVerified = true,
				attributes = new Dictionary<string, string[]>
				{
					["tenant"] = new[] { tenantClientId }
				}
			});

		if (create.StatusCode is not HttpStatusCode.Created and not HttpStatusCode.NoContent)
		{
			create.EnsureSuccessStatusCode();
		}

		users = await http.GetFromJsonAsync<List<UserRepresentation>>(
			$"admin/realms/{Uri.EscapeDataString(realmName)}/users?username={Uri.EscapeDataString(userData.Username)}&exact=true");
		userId = users?.FirstOrDefault()?.Id;
	}

	if (string.IsNullOrWhiteSpace(userId))
	{
		throw new InvalidOperationException($"Could not resolve Keycloak user id for '{userData.Username}'.");
	}

	// Set password
	using var reset = await http.PutAsJsonAsync(
		$"admin/realms/{Uri.EscapeDataString(realmName)}/users/{Uri.EscapeDataString(userId)}/reset-password",
		new
		{
			type = "password",
			value = userData.Password,
			temporary = false
		});

	reset.EnsureSuccessStatusCode();

	// Assign realm role to user
	await AssignRealmRoleToUserAsync(http, realmName, userId, userData.Role);
}

static async Task AssignRealmRoleToUserAsync(HttpClient http, string realmName, string userId, string roleName)
{
	// Get the role representation
	var role = await http.GetFromJsonAsync<RoleRepresentation>(
		$"admin/realms/{Uri.EscapeDataString(realmName)}/roles/{Uri.EscapeDataString(roleName)}");

	if (role?.Id is null)
	{
		Console.WriteLine($"Warning: Role '{roleName}' not found, skipping role assignment.");
		return;
	}

	// Check if user already has the role
	var userRoles = await http.GetFromJsonAsync<List<RoleRepresentation>>(
		$"admin/realms/{Uri.EscapeDataString(realmName)}/users/{Uri.EscapeDataString(userId)}/role-mappings/realm");

	if (userRoles?.Any(r => r.Name == roleName) == true)
	{
		Console.WriteLine($"User already has role '{roleName}'.");
		return;
	}

	Console.WriteLine($"Assigning role '{roleName}' to user '{userId}'...");

	using var response = await http.PostAsJsonAsync(
		$"admin/realms/{Uri.EscapeDataString(realmName)}/users/{Uri.EscapeDataString(userId)}/role-mappings/realm",
		new[] { new { id = role.Id, name = role.Name } });

	response.EnsureSuccessStatusCode();
}

static async Task<bool> ResourceExistsAsync(HttpClient http, string relativePath)
{
	using var response = await http.GetAsync(relativePath);
	return response.StatusCode switch
	{
		HttpStatusCode.OK => true,
		HttpStatusCode.NotFound => false,
		_ => response.IsSuccessStatusCode
	};
}

record Tenant(string Prefix, string Name, string Industry)
{
	public string ClientId => Prefix;
}

record UserData(string Username, string FirstName, string LastName, string Email, string Role, string Password);

sealed record ClientRepresentation(string? Id, string? ClientId);
sealed record UserRepresentation(string? Id, string? Username);
sealed record RoleRepresentation(string? Id, string? Name);
