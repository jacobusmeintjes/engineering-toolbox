---
name: blazor-developer
description: Expert guidance for building modern Blazor applications including component development, state management, JavaScript interop, forms, validation, routing, authentication, and performance optimization for both Server and WebAssembly
---

# Blazor Developer Expert

## When to use this skill

Use this skill when:
- Building Blazor Server or WebAssembly applications
- Creating reusable Blazor components
- Implementing forms and validation
- Managing component state and lifecycle
- Working with JavaScript interop
- Implementing real-time features with SignalR
- Optimizing Blazor performance
- Handling authentication and authorization
- Building responsive, accessible UIs

## Blazor Fundamentals

### Blazor Hosting Models

**Blazor Server**
- Runs on server, UI updates via SignalR
- Fast initial load, lower bandwidth
- Full .NET runtime on server
- Requires persistent connection
- Best for: Internal apps, enterprise scenarios

**Blazor WebAssembly**
- Runs entirely in browser
- Can work offline (PWA)
- Higher initial download
- No server dependency after load
- Best for: Public apps, PWAs, static hosting

**Blazor United (.NET 8+)**
- Hybrid rendering (Server + WASM)
- Per-component or per-page render modes
- Best of both worlds

## Component Development

### Basic Component Structure

```razor
@page "/customers"
@using MyApp.Services
@inject ICustomerService CustomerService
@inject NavigationManager Navigation

<PageTitle>Customers</PageTitle>

<h1>Customer Management</h1>

@if (isLoading)
{
    <p><em>Loading...</em></p>
}
else if (customers is not null)
{
    <div class="table-responsive">
        <table class="table table-striped">
            <thead>
                <tr>
                    <th>Name</th>
                    <th>Email</th>
                    <th>Actions</th>
                </tr>
            </thead>
            <tbody>
                @foreach (var customer in customers)
                {
                    <tr>
                        <td>@customer.Name</td>
                        <td>@customer.Email</td>
                        <td>
                            <button class="btn btn-sm btn-primary" 
                                    @onclick="() => EditCustomer(customer.Id)">
                                Edit
                            </button>
                        </td>
                    </tr>
                }
            </tbody>
        </table>
    </div>
}

@code {
    private List<CustomerDto>? customers;
    private bool isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        await LoadCustomersAsync();
    }

    private async Task LoadCustomersAsync()
    {
        isLoading = true;
        try
        {
            customers = await CustomerService.GetAllAsync();
        }
        finally
        {
            isLoading = false;
        }
    }

    private void EditCustomer(string customerId)
    {
        Navigation.NavigateTo($"/customers/edit/{customerId}");
    }
}
```

### Reusable Components with Parameters

```razor
@* CustomerCard.razor *@
<div class="card @CssClass">
    <div class="card-body">
        <h5 class="card-title">@Customer.Name</h5>
        <p class="card-text">@Customer.Email</p>
        
        @if (ShowActions)
        {
            <div class="btn-group">
                <button class="btn btn-sm btn-primary" @onclick="HandleEdit">
                    Edit
                </button>
                <button class="btn btn-sm btn-danger" @onclick="HandleDelete">
                    Delete
                </button>
            </div>
        }
        
        @ChildContent
    </div>
</div>

@code {
    [Parameter, EditorRequired]
    public CustomerDto Customer { get; set; } = default!;
    
    [Parameter]
    public bool ShowActions { get; set; } = true;
    
    [Parameter]
    public string CssClass { get; set; } = string.Empty;
    
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
    
    [Parameter]
    public EventCallback<string> OnEdit { get; set; }
    
    [Parameter]
    public EventCallback<string> OnDelete { get; set; }
    
    private async Task HandleEdit()
    {
        await OnEdit.InvokeAsync(Customer.Id);
    }
    
    private async Task HandleDelete()
    {
        await OnDelete.InvokeAsync(Customer.Id);
    }
}
```

### Component Lifecycle

```razor
@implements IDisposable
@inject ILogger<MyComponent> Logger

<h3>Component Lifecycle Demo</h3>

@code {
    // 1. Constructor
    public MyComponent()
    {
        Logger.LogDebug("Constructor called");
    }
    
    // 2. SetParametersAsync - Parameters set
    public override Task SetParametersAsync(ParameterView parameters)
    {
        Logger.LogDebug("SetParametersAsync called");
        return base.SetParametersAsync(parameters);
    }
    
    // 3. OnInitialized/OnInitializedAsync - Component initialized
    protected override void OnInitialized()
    {
        Logger.LogDebug("OnInitialized called");
    }
    
    protected override async Task OnInitializedAsync()
    {
        Logger.LogDebug("OnInitializedAsync called");
        // Load initial data here
        await LoadDataAsync();
    }
    
    // 4. OnParametersSet/OnParametersSetAsync - After parameters set
    protected override void OnParametersSet()
    {
        Logger.LogDebug("OnParametersSet called");
    }
    
    protected override async Task OnParametersSetAsync()
    {
        Logger.LogDebug("OnParametersSetAsync called");
        // React to parameter changes
        await RefreshDataAsync();
    }
    
    // 5. OnAfterRender/OnAfterRenderAsync - After rendering
    protected override void OnAfterRender(bool firstRender)
    {
        Logger.LogDebug("OnAfterRender called, firstRender: {FirstRender}", firstRender);
        
        if (firstRender)
        {
            // One-time setup after first render
        }
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        Logger.LogDebug("OnAfterRenderAsync called");
        
        if (firstRender)
        {
            // JS interop that needs rendered DOM
            await JsRuntime.InvokeVoidAsync("initializeComponent");
        }
    }
    
    // 6. Dispose - Component disposed
    public void Dispose()
    {
        Logger.LogDebug("Dispose called");
        // Clean up resources
    }
}
```

## State Management

### Cascading Parameters

```razor
@* Layout or Parent Component *@
<CascadingValue Value="currentUser">
    <CascadingValue Value="theme">
        @Body
    </CascadingValue>
</CascadingValue>

@code {
    private UserInfo currentUser = new();
    private ThemeInfo theme = new();
}

@* Child Component *@
@code {
    [CascadingParameter]
    public UserInfo CurrentUser { get; set; } = default!;
    
    [CascadingParameter]
    public ThemeInfo Theme { get; set; } = default!;
}
```

### State Container Pattern

```csharp
// AppState.cs
public class AppState
{
    private UserInfo? _currentUser;
    
    public UserInfo? CurrentUser
    {
        get => _currentUser;
        set
        {
            _currentUser = value;
            NotifyStateChanged();
        }
    }
    
    public event Action? OnChange;
    
    private void NotifyStateChanged() => OnChange?.Invoke();
}

// Program.cs
builder.Services.AddScoped<AppState>();

// Component usage
@implements IDisposable
@inject AppState AppState

<p>Welcome, @AppState.CurrentUser?.Name</p>

@code {
    protected override void OnInitialized()
    {
        AppState.OnChange += StateHasChanged;
    }
    
    public void Dispose()
    {
        AppState.OnChange -= StateHasChanged;
    }
}
```

### Fluxor (Redux Pattern)

```csharp
// State
public record CustomerState
{
    public bool IsLoading { get; init; }
    public List<CustomerDto> Customers { get; init; } = new();
    public string? ErrorMessage { get; init; }
}

// Actions
public record LoadCustomersAction;
public record LoadCustomersSuccessAction(List<CustomerDto> Customers);
public record LoadCustomersFailureAction(string ErrorMessage);

// Reducer
public static class CustomerReducers
{
    [ReducerMethod]
    public static CustomerState ReduceLoadCustomersAction(
        CustomerState state, 
        LoadCustomersAction action)
    {
        return state with { IsLoading = true, ErrorMessage = null };
    }
    
    [ReducerMethod]
    public static CustomerState ReduceLoadCustomersSuccessAction(
        CustomerState state,
        LoadCustomersSuccessAction action)
    {
        return state with 
        { 
            IsLoading = false, 
            Customers = action.Customers 
        };
    }
    
    [ReducerMethod]
    public static CustomerState ReduceLoadCustomersFailureAction(
        CustomerState state,
        LoadCustomersFailureAction action)
    {
        return state with 
        { 
            IsLoading = false, 
            ErrorMessage = action.ErrorMessage 
        };
    }
}

// Effects
public class CustomerEffects
{
    private readonly ICustomerService _customerService;
    
    public CustomerEffects(ICustomerService customerService)
    {
        _customerService = customerService;
    }
    
    [EffectMethod]
    public async Task HandleLoadCustomersAction(
        LoadCustomersAction action, 
        IDispatcher dispatcher)
    {
        try
        {
            var customers = await _customerService.GetAllAsync();
            dispatcher.Dispatch(new LoadCustomersSuccessAction(customers));
        }
        catch (Exception ex)
        {
            dispatcher.Dispatch(new LoadCustomersFailureAction(ex.Message));
        }
    }
}

// Feature
public class CustomerFeature : Feature<CustomerState>
{
    public override string GetName() => "Customer";
    
    protected override CustomerState GetInitialState()
    {
        return new CustomerState();
    }
}

// Component usage
@inherits FluxorComponent
@inject IState<CustomerState> CustomerState
@inject IDispatcher Dispatcher

<h3>Customers</h3>

@if (CustomerState.Value.IsLoading)
{
    <p>Loading...</p>
}
else if (CustomerState.Value.ErrorMessage is not null)
{
    <div class="alert alert-danger">@CustomerState.Value.ErrorMessage</div>
}
else
{
    @foreach (var customer in CustomerState.Value.Customers)
    {
        <CustomerCard Customer="customer" />
    }
}

@code {
    protected override void OnInitialized()
    {
        base.OnInitialized();
        Dispatcher.Dispatch(new LoadCustomersAction());
    }
}
```

## Forms and Validation

### EditForm with Data Annotations

```razor
@page "/customer/create"
@inject ICustomerService CustomerService
@inject NavigationManager Navigation

<h3>Create Customer</h3>

<EditForm Model="model" OnValidSubmit="HandleValidSubmit">
    <DataAnnotationsValidator />
    <ValidationSummary />
    
    <div class="mb-3">
        <label for="name" class="form-label">Name</label>
        <InputText id="name" @bind-Value="model.Name" class="form-control" />
        <ValidationMessage For="@(() => model.Name)" />
    </div>
    
    <div class="mb-3">
        <label for="email" class="form-label">Email</label>
        <InputText id="email" @bind-Value="model.Email" class="form-control" 
                   type="email" />
        <ValidationMessage For="@(() => model.Email)" />
    </div>
    
    <div class="mb-3">
        <label for="tier" class="form-label">Tier</label>
        <InputSelect id="tier" @bind-Value="model.Tier" class="form-select">
            <option value="">Select tier...</option>
            @foreach (var tier in Enum.GetValues<CustomerTier>())
            {
                <option value="@tier">@tier</option>
            }
        </InputSelect>
        <ValidationMessage For="@(() => model.Tier)" />
    </div>
    
    <div class="mb-3">
        <label for="birthdate" class="form-label">Birth Date</label>
        <InputDate id="birthdate" @bind-Value="model.BirthDate" class="form-control" />
        <ValidationMessage For="@(() => model.BirthDate)" />
    </div>
    
    <div class="mb-3 form-check">
        <InputCheckbox id="newsletter" @bind-Value="model.SubscribeToNewsletter" 
                       class="form-check-input" />
        <label for="newsletter" class="form-check-label">Subscribe to newsletter</label>
    </div>
    
    <button type="submit" class="btn btn-primary" disabled="@isSaving">
        @(isSaving ? "Saving..." : "Create Customer")
    </button>
</EditForm>

@code {
    private CreateCustomerModel model = new();
    private bool isSaving;
    
    private async Task HandleValidSubmit()
    {
        isSaving = true;
        try
        {
            var customer = await CustomerService.CreateAsync(model);
            Navigation.NavigateTo($"/customers/{customer.Id}");
        }
        catch (Exception ex)
        {
            // Show error message
        }
        finally
        {
            isSaving = false;
        }
    }
}
```

### Custom Validation

```csharp
public class CreateCustomerModel
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [CustomValidation(typeof(CreateCustomerModel), nameof(ValidateUniqueEmail))]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public CustomerTier? Tier { get; set; }
    
    [Required]
    [Range(typeof(DateTime), "1900-01-01", "2020-01-01", 
           ErrorMessage = "Birth date must be between 1900 and 2020")]
    public DateTime? BirthDate { get; set; }
    
    public bool SubscribeToNewsletter { get; set; }
    
    public static ValidationResult? ValidateUniqueEmail(
        string email, 
        ValidationContext context)
    {
        // Custom validation logic
        var service = context.GetService<ICustomerService>();
        
        if (service?.IsEmailTakenAsync(email).Result == true)
        {
            return new ValidationResult("Email is already in use");
        }
        
        return ValidationResult.Success;
    }
}
```

### FluentValidation Integration

```csharp
public class CreateCustomerModelValidator : AbstractValidator<CreateCustomerModel>
{
    public CreateCustomerModelValidator(ICustomerService customerService)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .Length(2, 100)
            .WithMessage("Name must be between 2 and 100 characters");
        
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MustAsync(async (email, cancellation) => 
                !await customerService.IsEmailTakenAsync(email))
            .WithMessage("Email is already in use");
        
        RuleFor(x => x.Tier)
            .NotNull()
            .IsInEnum();
        
        RuleFor(x => x.BirthDate)
            .NotNull()
            .LessThan(DateTime.Today.AddYears(-18))
            .WithMessage("Must be at least 18 years old");
    }
}

// Component usage
<EditForm Model="model" OnValidSubmit="HandleValidSubmit">
    <FluentValidationValidator />
    <ValidationSummary />
    
    @* Form fields *@
</EditForm>
```

## JavaScript Interop

### Calling JavaScript from .NET

```razor
@inject IJSRuntime JS

<button @onclick="ShowAlert">Show Alert</button>
<button @onclick="FocusInput">Focus Input</button>
<input @ref="inputElement" />

@code {
    private ElementReference inputElement;
    
    private async Task ShowAlert()
    {
        await JS.InvokeVoidAsync("alert", "Hello from Blazor!");
    }
    
    private async Task FocusInput()
    {
        await JS.InvokeVoidAsync("focusElement", inputElement);
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Initialize JavaScript library
            await JS.InvokeVoidAsync("initializeChart", "myChart");
        }
    }
}
```

```javascript
// wwwroot/js/interop.js
window.focusElement = (element) => {
    if (element) {
        element.focus();
    }
};

window.initializeChart = (elementId) => {
    // Initialize chart library
    const ctx = document.getElementById(elementId);
    new Chart(ctx, { /* config */ });
};

window.getLocalStorage = (key) => {
    return localStorage.getItem(key);
};

window.setLocalStorage = (key, value) => {
    localStorage.setItem(key, value);
};
```

### Calling .NET from JavaScript

```csharp
public class JsInteropService
{
    [JSInvokable]
    public static Task<string> GetServerTime()
    {
        return Task.FromResult(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
    }
    
    [JSInvokable]
    public static async Task<bool> ConfirmActionAsync(string message)
    {
        // This would typically show a modal or confirmation dialog
        await Task.Delay(100);
        return true;
    }
}
```

```javascript
// Call .NET static method
const serverTime = await DotNet.invokeMethodAsync('MyApp', 'GetServerTime');

// Call .NET instance method
const dotNetHelper = DotNet.createJSObjectReference(componentInstance);
const result = await dotNetHelper.invokeMethodAsync('ConfirmActionAsync', 'Are you sure?');
```

### Custom JS Interop Module

```csharp
public class LocalStorageService : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask;
    
    public LocalStorageService(IJSRuntime jsRuntime)
    {
        _moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./js/storage.js").AsTask());
    }
    
    public async ValueTask<string?> GetItemAsync(string key)
    {
        var module = await _moduleTask.Value;
        return await module.InvokeAsync<string?>("getItem", key);
    }
    
    public async ValueTask SetItemAsync(string key, string value)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("setItem", key, value);
    }
    
    public async ValueTask RemoveItemAsync(string key)
    {
        var module = await _moduleTask.Value;
        await module.InvokeVoidAsync("removeItem", key);
    }
    
    public async ValueTask DisposeAsync()
    {
        if (_moduleTask.IsValueCreated)
        {
            var module = await _moduleTask.Value;
            await module.DisposeAsync();
        }
    }
}
```

```javascript
// wwwroot/js/storage.js
export function getItem(key) {
    return localStorage.getItem(key);
}

export function setItem(key, value) {
    localStorage.setItem(key, value);
}

export function removeItem(key) {
    localStorage.removeItem(key);
}
```

## Authentication and Authorization

### Authentication Setup

```csharp
// Program.cs
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.Authority = builder.Configuration["Oidc:Authority"];
    options.ClientId = builder.Configuration["Oidc:ClientId"];
    options.ClientSecret = builder.Configuration["Oidc:ClientSecret"];
    options.ResponseType = "code";
    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
});

builder.Services.AddAuthorizationCore(options =>
{
    options.AddPolicy("AdminOnly", policy => 
        policy.RequireRole("Admin"));
    
    options.AddPolicy("CanEditCustomers", policy =>
        policy.RequireClaim("permission", "customers.edit"));
});
```

### Authorized Components

```razor
@page "/admin"
@attribute [Authorize(Roles = "Admin")]

<h3>Admin Dashboard</h3>

<AuthorizeView Roles="Admin">
    <Authorized>
        <p>Welcome, @context.User.Identity?.Name</p>
        <AdminPanel />
    </Authorized>
    <NotAuthorized>
        <p>You are not authorized to view this page.</p>
    </NotAuthorized>
</AuthorizeView>
```

```razor
@* Inline authorization *@
<AuthorizeView Policy="CanEditCustomers">
    <Authorized>
        <button @onclick="EditCustomer">Edit</button>
    </Authorized>
    <Authorizing>
        <p>Checking permissions...</p>
    </Authorizing>
    <NotAuthorized>
        <p>You don't have permission to edit customers.</p>
    </NotAuthorized>
</AuthorizeView>
```

### Getting Current User

```razor
@inject AuthenticationStateProvider AuthStateProvider

<p>Current user: @userName</p>

@code {
    private string? userName;
    
    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        userName = authState.User.Identity?.Name;
        
        // Get claims
        var emailClaim = authState.User.FindFirst(ClaimTypes.Email);
        var roleClaims = authState.User.FindAll(ClaimTypes.Role);
    }
}
```

## Real-Time with SignalR

### SignalR Hub

```csharp
public class NotificationHub : Hub
{
    public async Task SendNotificationToUser(string userId, string message)
    {
        await Clients.User(userId).SendAsync("ReceiveNotification", message);
    }
    
    public async Task BroadcastNotification(string message)
    {
        await Clients.All.SendAsync("ReceiveNotification", message);
    }
    
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        await Clients.Group(groupName).SendAsync("UserJoined", Context.UserIdentifier);
    }
}

// Program.cs
builder.Services.AddSignalR();

app.MapHub<NotificationHub>("/notificationhub");
```

### SignalR Client in Blazor

```razor
@inject NavigationManager Navigation
@implements IAsyncDisposable

<h3>Real-Time Notifications</h3>

<ul>
    @foreach (var notification in notifications)
    {
        <li>@notification</li>
    }
</ul>

@code {
    private HubConnection? hubConnection;
    private List<string> notifications = new();
    
    protected override async Task OnInitializedAsync()
    {
        hubConnection = new HubConnectionBuilder()
            .WithUrl(Navigation.ToAbsoluteUri("/notificationhub"))
            .WithAutomaticReconnect()
            .Build();
        
        hubConnection.On<string>("ReceiveNotification", notification =>
        {
            notifications.Add(notification);
            InvokeAsync(StateHasChanged);
        });
        
        await hubConnection.StartAsync();
    }
    
    private async Task SendNotification(string message)
    {
        if (hubConnection is not null)
        {
            await hubConnection.SendAsync("BroadcastNotification", message);
        }
    }
    
    public async ValueTask DisposeAsync()
    {
        if (hubConnection is not null)
        {
            await hubConnection.DisposeAsync();
        }
    }
}
```

## Performance Optimization

### Virtualization

```razor
@using Microsoft.AspNetCore.Components.Web.Virtualization

<Virtualize Items="customers" Context="customer">
    <CustomerCard Customer="customer" />
</Virtualize>

@* Or with ItemsProvider for large datasets *@
<Virtualize ItemsProvider="LoadCustomersAsync" Context="customer">
    <CustomerCard Customer="customer" />
</Virtualize>

@code {
    private async ValueTask<ItemsProviderResult<CustomerDto>> LoadCustomersAsync(
        ItemsProviderRequest request)
    {
        var customers = await CustomerService.GetPagedAsync(
            request.StartIndex, 
            request.Count);
        
        var totalCount = await CustomerService.GetTotalCountAsync();
        
        return new ItemsProviderResult<CustomerDto>(customers, totalCount);
    }
}
```

### Streaming Rendering (.NET 8+)

```razor
@page "/customers"
@attribute [StreamRendering]

<h3>Customers</h3>

@if (customers is null)
{
    <p>Loading...</p>
}
else
{
    @foreach (var customer in customers)
    {
        <CustomerCard Customer="customer" />
    }
}

@code {
    private List<CustomerDto>? customers;
    
    protected override async Task OnInitializedAsync()
    {
        // Initial render happens immediately with loading state
        await Task.Yield();
        
        // Then data loads and component re-renders
        customers = await CustomerService.GetAllAsync();
    }
}
```

### Component Memoization

```razor
@* Only re-renders when CustomerId parameter changes *@
@code {
    [Parameter]
    public string CustomerId { get; set; } = string.Empty;
    
    protected override bool ShouldRender()
    {
        // Custom logic to decide if component should re-render
        return true;
    }
}
```

### Lazy Loading

```razor
@page "/reports"

<button @onclick="LoadReportComponent">Load Report</button>

@if (showReport)
{
    <div>
        @* Lazy-loaded component *@
        @((RenderFragment)(builder =>
        {
            builder.OpenComponent(0, reportComponentType);
            builder.CloseComponent();
        }))
    </div>
}

@code {
    private bool showReport;
    private Type? reportComponentType;
    
    private async Task LoadReportComponent()
    {
        // Dynamically load assembly
        var assembly = await AssemblyLoader.LoadAssemblyAsync("Reports.dll");
        reportComponentType = assembly.GetType("Reports.CustomerReport");
        showReport = true;
    }
}
```

## Error Handling

### Error Boundary

```razor
<ErrorBoundary>
    <ChildContent>
        <CustomerList />
    </ChildContent>
    <ErrorContent Context="ex">
        <div class="alert alert-danger">
            <h4>An error occurred</h4>
            <p>@ex.Message</p>
            <button class="btn btn-primary" @onclick="RecoverAsync">
                Try Again
            </button>
        </div>
    </ErrorContent>
</ErrorBoundary>

@code {
    private ErrorBoundary? errorBoundary;
    
    private async Task RecoverAsync()
    {
        errorBoundary?.Recover();
        await LoadDataAsync();
    }
}
```

## Routing and Navigation

### Advanced Routing

```razor
@page "/customers/{CustomerId}"
@page "/customers/{CustomerId}/orders"
@page "/customers/{CustomerId}/orders/{OrderId}"

@code {
    [Parameter]
    public string CustomerId { get; set; } = string.Empty;
    
    [Parameter]
    public string? OrderId { get; set; }
    
    [SupplyParameterFromQuery(Name = "page")]
    public int? PageNumber { get; set; }
}
```

### Programmatic Navigation

```csharp
@inject NavigationManager Navigation

private void NavigateToCustomer(string customerId)
{
    Navigation.NavigateTo($"/customers/{customerId}");
}

private void NavigateWithQuery()
{
    var uri = Navigation.GetUriWithQueryParameters(
        "/customers",
        new Dictionary<string, object?>
        {
            ["page"] = 1,
            ["search"] = "john"
        });
    
    Navigation.NavigateTo(uri);
}
```

## Best Practices

### Component Organization
```
Components/
├── Pages/           # Routable pages
├── Shared/          # Shared components
├── Layout/          # Layout components
└── Forms/           # Form components

Services/            # Business logic
Models/              # Data models
ViewModels/          # View models
```

### Dependency Injection Scopes
- **Singleton**: Shared across all users (use sparingly)
- **Scoped**: Per-circuit (Blazor Server) or per-page (WASM)
- **Transient**: New instance every time

### Performance Tips
1. Use `@key` directive for list rendering
2. Implement `ShouldRender()` for expensive components
3. Use virtualization for large lists
4. Minimize JavaScript interop calls
5. Use streaming rendering for large pages
6. Prerender pages when possible

## Resources

- [Blazor Documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor/)
- [Blazor University](https://blazor-university.com/)
- [Awesome Blazor](https://github.com/AdrienTorris/awesome-blazor)
- [Blazor Component Libraries](https://github.com/topics/blazor-components)
