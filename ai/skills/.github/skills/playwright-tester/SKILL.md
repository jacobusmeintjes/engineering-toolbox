---
name: playwright-ui-tester
description: Expert guidance for end-to-end UI testing with Playwright including page object patterns, accessibility testing, visual regression, mobile testing, CI/CD integration, and best practices for reliable automated tests
---

# Playwright UI Tester Expert

## When to use this skill

Use this skill when:
- Writing end-to-end UI tests with Playwright
- Implementing page object patterns
- Testing accessibility with Playwright
- Performing visual regression testing
- Testing responsive designs and mobile
- Debugging flaky tests
- Setting up Playwright in CI/CD
- Testing authentication flows
- Working with iframes, file uploads, downloads

## Playwright Fundamentals

### Basic Test Structure

```csharp
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace MyApp.E2ETests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class CustomerTests : PageTest
{
    [SetUp]
    public async Task Setup()
    {
        await Page.GotoAsync("https://localhost:5001");
    }
    
    [Test]
    public async Task UserCanCreateCustomer()
    {
        // Navigate to create customer page
        await Page.ClickAsync("text=New Customer");
        
        // Fill in form
        await Page.FillAsync("[name='name']", "John Doe");
        await Page.FillAsync("[name='email']", "john@example.com");
        await Page.SelectOptionAsync("[name='tier']", "Premium");
        
        // Submit form
        await Page.ClickAsync("button[type='submit']");
        
        // Assert success
        await Expect(Page.Locator("text=Customer created successfully")).ToBeVisibleAsync();
        
        // Verify redirect to customer detail page
        await Expect(Page).ToHaveURLAsync(new Regex(@"/customers/[\w-]+"));
        
        // Verify customer details are displayed
        await Expect(Page.Locator("h1")).ToContainTextAsync("John Doe");
    }
    
    [Test]
    public async Task FormValidationWorks()
    {
        await Page.ClickAsync("text=New Customer");
        
        // Try to submit without filling required fields
        await Page.ClickAsync("button[type='submit']");
        
        // Assert validation messages
        await Expect(Page.Locator("text=Name is required")).ToBeVisibleAsync();
        await Expect(Page.Locator("text=Email is required")).ToBeVisibleAsync();
    }
}
```

### Page Object Pattern

```csharp
// Pages/CustomerListPage.cs
public class CustomerListPage
{
    private readonly IPage _page;
    
    public CustomerListPage(IPage page)
    {
        _page = page;
    }
    
    // Locators
    private ILocator NewCustomerButton => _page.Locator("button:has-text('New Customer')");
    private ILocator SearchInput => _page.Locator("[placeholder='Search customers...']");
    private ILocator CustomerRows => _page.Locator("table tbody tr");
    
    // Actions
    public async Task NavigateAsync()
    {
        await _page.GotoAsync("/customers");
    }
    
    public async Task ClickNewCustomerAsync()
    {
        await NewCustomerButton.ClickAsync();
    }
    
    public async Task SearchAsync(string searchTerm)
    {
        await SearchInput.FillAsync(searchTerm);
        await SearchInput.PressAsync("Enter");
    }
    
    public async Task<CustomerRow> GetCustomerRowAsync(int index)
    {
        var row = CustomerRows.Nth(index);
        return new CustomerRow(row);
    }
    
    public async Task<int> GetCustomerCountAsync()
    {
        return await CustomerRows.CountAsync();
    }
    
    // Assertions
    public async Task AssertCustomerExists(string customerName)
    {
        await Expect(_page.Locator($"td:has-text('{customerName}')")).ToBeVisibleAsync();
    }
}

public class CustomerRow
{
    private readonly ILocator _row;
    
    public CustomerRow(ILocator row)
    {
        _row = row;
    }
    
    public ILocator Name => _row.Locator("td:nth-child(1)");
    public ILocator Email => _row.Locator("td:nth-child(2)");
    public ILocator EditButton => _row.Locator("button:has-text('Edit')");
    public ILocator DeleteButton => _row.Locator("button:has-text('Delete')");
    
    public async Task ClickEditAsync()
    {
        await EditButton.ClickAsync();
    }
    
    public async Task ClickDeleteAsync()
    {
        await DeleteButton.ClickAsync();
    }
}

// Pages/CreateCustomerPage.cs
public class CreateCustomerPage
{
    private readonly IPage _page;
    
    public CreateCustomerPage(IPage page)
    {
        _page = page;
    }
    
    // Locators
    private ILocator NameInput => _page.Locator("[name='name']");
    private ILocator EmailInput => _page.Locator("[name='email']");
    private ILocator TierSelect => _page.Locator("[name='tier']");
    private ILocator NewsletterCheckbox => _page.Locator("[name='newsletter']");
    private ILocator SubmitButton => _page.Locator("button[type='submit']");
    private ILocator CancelButton => _page.Locator("button:has-text('Cancel')");
    
    // Actions
    public async Task FillFormAsync(CustomerFormData data)
    {
        await NameInput.FillAsync(data.Name);
        await EmailInput.FillAsync(data.Email);
        
        if (!string.IsNullOrEmpty(data.Tier))
        {
            await TierSelect.SelectOptionAsync(data.Tier);
        }
        
        if (data.SubscribeToNewsletter)
        {
            await NewsletterCheckbox.CheckAsync();
        }
    }
    
    public async Task SubmitAsync()
    {
        await SubmitButton.ClickAsync();
    }
    
    public async Task CancelAsync()
    {
        await CancelButton.ClickAsync();
    }
    
    // Assertions
    public async Task AssertValidationError(string fieldName, string expectedMessage)
    {
        var error = _page.Locator($"[name='{fieldName}'] ~ .validation-message");
        await Expect(error).ToContainTextAsync(expectedMessage);
    }
}

public record CustomerFormData(
    string Name,
    string Email,
    string? Tier = null,
    bool SubscribeToNewsletter = false);

// Test using Page Objects
[TestFixture]
public class CustomerPageObjectTests : PageTest
{
    private CustomerListPage _listPage = null!;
    private CreateCustomerPage _createPage = null!;
    
    [SetUp]
    public async Task Setup()
    {
        _listPage = new CustomerListPage(Page);
        _createPage = new CreateCustomerPage(Page);
        
        await _listPage.NavigateAsync();
    }
    
    [Test]
    public async Task CanCreateCustomerUsingPageObjects()
    {
        await _listPage.ClickNewCustomerAsync();
        
        var customerData = new CustomerFormData(
            Name: "Jane Smith",
            Email: "jane@example.com",
            Tier: "Premium",
            SubscribeToNewsletter: true
        );
        
        await _createPage.FillFormAsync(customerData);
        await _createPage.SubmitAsync();
        
        // Verify customer appears in list
        await _listPage.NavigateAsync();
        await _listPage.AssertCustomerExists("Jane Smith");
    }
}
```

## Locator Strategies

### Best Practices for Locators

```csharp
// ✅ GOOD: Use role-based selectors (accessible and semantic)
await Page.GetByRole(AriaRole.Button, new() { Name = "Submit" }).ClickAsync();
await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email" }).FillAsync("test@example.com");
await Page.GetByRole(AriaRole.Heading, new() { Level = 1 }).ToContainTextAsync("Welcome");

// ✅ GOOD: Use test IDs (data-testid attributes)
await Page.GetByTestId("customer-name-input").FillAsync("John Doe");
await Page.GetByTestId("submit-button").ClickAsync();

// ✅ GOOD: Use labels
await Page.GetByLabel("Email address").FillAsync("test@example.com");
await Page.GetByLabel("Subscribe to newsletter").CheckAsync();

// ✅ GOOD: Use placeholder text
await Page.GetByPlaceholder("Search customers...").FillAsync("John");

// ⚠️ OKAY: Use text content (can break with text changes)
await Page.Locator("text=New Customer").ClickAsync();

// ❌ AVOID: CSS selectors (brittle, not semantic)
await Page.Locator("div.container > form > input:nth-child(2)").FillAsync("test");

// ❌ AVOID: XPath (hard to read and maintain)
await Page.Locator("//div[@class='form']//input[@type='text'][2]").FillAsync("test");
```

### Chaining and Filtering Locators

```csharp
// Find button within a specific section
await Page
    .Locator("section.customer-details")
    .Locator("button:has-text('Edit')")
    .ClickAsync();

// Filter locators
var activeCustomers = Page
    .Locator("tr.customer-row")
    .Filter(new() { HasText = "Active" });

await Expect(activeCustomers).ToHaveCountAsync(5);

// Get nth element
var firstCustomer = Page.Locator("tr.customer-row").First;
var thirdCustomer = Page.Locator("tr.customer-row").Nth(2);
var lastCustomer = Page.Locator("tr.customer-row").Last;

// Has/HasNot
var premiumCustomers = Page
    .Locator("tr.customer-row")
    .Filter(new() { Has = Page.Locator("span.badge:has-text('Premium')") });
```

## Waiting and Assertions

### Auto-Waiting

```csharp
// Playwright automatically waits for:
// - Element to be attached to DOM
// - Element to be visible
// - Element to be stable (not animating)
// - Element to receive events (not covered)
// - Element to be enabled

// All these actions auto-wait:
await Page.ClickAsync("button");
await Page.FillAsync("input", "text");
await Page.CheckAsync("checkbox");
await Page.SelectOptionAsync("select", "value");

// Manual waiting (when auto-wait isn't enough)
await Page.WaitForSelectorAsync("text=Customer created");
await Page.WaitForURLAsync("**/customers/**");
await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

// Wait for function
await Page.WaitForFunctionAsync("() => document.querySelectorAll('.customer-row').length > 5");
```

### Assertions

```csharp
// Element state assertions
await Expect(Page.Locator("button")).ToBeVisibleAsync();
await Expect(Page.Locator("button")).ToBeHiddenAsync();
await Expect(Page.Locator("button")).ToBeEnabledAsync();
await Expect(Page.Locator("button")).ToBeDisabledAsync();
await Expect(Page.Locator("input")).ToBeEditableAsync();
await Expect(Page.Locator("input")).ToBeFocusedAsync();
await Expect(Page.Locator("checkbox")).ToBeCheckedAsync();

// Content assertions
await Expect(Page.Locator("h1")).ToContainTextAsync("Customer");
await Expect(Page.Locator("h1")).ToHaveTextAsync("Customer Details");
await Expect(Page.Locator("input")).ToHaveValueAsync("John Doe");
await Expect(Page.Locator(".error")).ToHaveTextAsync(new Regex("required"));

// Count assertions
await Expect(Page.Locator("tr.customer-row")).ToHaveCountAsync(10);

// URL assertions
await Expect(Page).ToHaveURLAsync("https://example.com/customers");
await Expect(Page).ToHaveURLAsync(new Regex(@"/customers/\d+"));

// Attribute assertions
await Expect(Page.Locator("button")).ToHaveAttributeAsync("disabled", "");
await Expect(Page.Locator("a")).ToHaveAttributeAsync("href", "/customers");

// CSS assertions
await Expect(Page.Locator(".alert")).ToHaveCSSAsync("background-color", "rgb(255, 0, 0)");

// Screenshot assertions
await Expect(Page.Locator(".chart")).ToHaveScreenshotAsync("chart.png");
```

## Authentication Testing

### Reusable Authentication State

```csharp
// Setup/AuthSetup.cs
[SetUpFixture]
public class AuthSetup
{
    [OneTimeSetUp]
    public async Task GlobalSetup()
    {
        using var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync();
        var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        
        // Perform login
        await page.GotoAsync("https://localhost:5001/login");
        await page.GetByLabel("Email").FillAsync("admin@example.com");
        await page.GetByLabel("Password").FillAsync("Admin123!");
        await page.GetByRole(AriaRole.Button, new() { Name = "Sign In" }).ClickAsync();
        
        // Wait for redirect
        await page.WaitForURLAsync("**/dashboard");
        
        // Save authentication state
        await context.StorageStateAsync(new()
        {
            Path = "auth.json"
        });
        
        await browser.CloseAsync();
    }
}

// Test using saved auth state
[TestFixture]
public class AuthenticatedTests : PageTest
{
    public override BrowserNewContextOptions ContextOptions()
    {
        return new BrowserNewContextOptions
        {
            StorageStatePath = "auth.json"
        };
    }
    
    [Test]
    public async Task CanAccessProtectedPage()
    {
        await Page.GotoAsync("/admin");
        
        // Should be authenticated
        await Expect(Page.Locator("text=Admin Dashboard")).ToBeVisibleAsync();
    }
}
```

### Testing Different User Roles

```csharp
public class AuthHelper
{
    public static async Task<string> LoginAs(IPage page, UserRole role)
    {
        var credentials = role switch
        {
            UserRole.Admin => ("admin@example.com", "Admin123!"),
            UserRole.Manager => ("manager@example.com", "Manager123!"),
            UserRole.User => ("user@example.com", "User123!"),
            _ => throw new ArgumentException("Invalid role")
        };
        
        await page.GotoAsync("/login");
        await page.GetByLabel("Email").FillAsync(credentials.Item1);
        await page.GetByLabel("Password").FillAsync(credentials.Item2);
        await page.GetByRole(AriaRole.Button, new() { Name = "Sign In" }).ClickAsync();
        
        // Save and return storage state
        var context = page.Context;
        var statePath = $"auth-{role}.json";
        await context.StorageStateAsync(new() { Path = statePath });
        return statePath;
    }
}

[Test]
public async Task AdminCanDeleteCustomers()
{
    var statePath = await AuthHelper.LoginAs(Page, UserRole.Admin);
    
    await Page.GotoAsync("/customers");
    await Page.Locator("button.delete-customer").First.ClickAsync();
    await Page.Locator("button:has-text('Confirm')").ClickAsync();
    
    await Expect(Page.Locator("text=Customer deleted")).ToBeVisibleAsync();
}

[Test]
public async Task RegularUserCannotDeleteCustomers()
{
    var statePath = await AuthHelper.LoginAs(Page, UserRole.User);
    
    await Page.GotoAsync("/customers");
    
    // Delete button should not be visible
    await Expect(Page.Locator("button.delete-customer")).ToBeHiddenAsync();
}
```

## Accessibility Testing

### Accessibility Scans

```csharp
using Deque.AxeCore.Playwright;

[Test]
public async Task PageShouldBeAccessible()
{
    await Page.GotoAsync("/customers");
    
    // Run accessibility scan
    var results = await Page.RunAxe();
    
    // Assert no violations
    Assert.That(results.Violations, Is.Empty, 
        $"Accessibility violations found: {string.Join(", ", results.Violations.Select(v => v.Id))}");
}

[Test]
public async Task FormShouldMeetWCAGStandards()
{
    await Page.GotoAsync("/customers/create");
    
    // Run scan with specific WCAG level
    var results = await Page.RunAxe(new AxeRunOptions
    {
        RunOnly = new RunOnly
        {
            Type = "tag",
            Values = new[] { "wcag2a", "wcag2aa" }
        }
    });
    
    // Check for violations
    if (results.Violations.Any())
    {
        foreach (var violation in results.Violations)
        {
            Console.WriteLine($"Violation: {violation.Id}");
            Console.WriteLine($"Impact: {violation.Impact}");
            Console.WriteLine($"Description: {violation.Description}");
            Console.WriteLine($"Help: {violation.HelpUrl}");
            Console.WriteLine();
        }
    }
    
    Assert.That(results.Violations, Is.Empty);
}

[Test]
public async Task KeyboardNavigationWorks()
{
    await Page.GotoAsync("/customers");
    
    // Tab through form
    await Page.Keyboard.PressAsync("Tab");
    var firstFocusedElement = await Page.EvaluateAsync<string>(
        "document.activeElement.tagName");
    
    Assert.That(firstFocusedElement, Is.EqualTo("INPUT"));
    
    // Enter key should submit
    await Page.Keyboard.PressAsync("Enter");
    await Expect(Page.Locator("text=Customer created")).ToBeVisibleAsync();
}

[Test]
public async Task ScreenReaderTextIsPresent()
{
    await Page.GotoAsync("/customers");
    
    // Check for aria-labels
    await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Close" }))
        .ToHaveAttributeAsync("aria-label", "Close dialog");
    
    // Check for screen reader only text
    var srOnlyText = Page.Locator(".sr-only:has-text('Customer list')");
    await Expect(srOnlyText).ToBeAttachedAsync();
}
```

## Visual Regression Testing

### Screenshot Comparison

```csharp
[Test]
public async Task CustomerListAppearanceIsCorrect()
{
    await Page.GotoAsync("/customers");
    
    // Wait for content to load
    await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    
    // Take screenshot and compare
    await Expect(Page).ToHaveScreenshotAsync("customer-list.png", new()
    {
        FullPage = true,
        MaxDiffPixels = 100 // Allow small differences
    });
}

[Test]
public async Task ComponentScreenshotMatches()
{
    await Page.GotoAsync("/customers/123");
    
    var customerCard = Page.Locator(".customer-card");
    
    // Screenshot specific element
    await Expect(customerCard).ToHaveScreenshotAsync("customer-card.png");
}

[Test]
public async Task ResponsiveDesignIsConsistent()
{
    await Page.GotoAsync("/customers");
    
    // Desktop
    await Page.SetViewportSizeAsync(1920, 1080);
    await Expect(Page).ToHaveScreenshotAsync("customer-list-desktop.png");
    
    // Tablet
    await Page.SetViewportSizeAsync(768, 1024);
    await Expect(Page).ToHaveScreenshotAsync("customer-list-tablet.png");
    
    // Mobile
    await Page.SetViewportSizeAsync(375, 667);
    await Expect(Page).ToHaveScreenshotAsync("customer-list-mobile.png");
}
```

## Mobile and Responsive Testing

### Device Emulation

```csharp
[Test]
public async Task MobileSiteWorks()
{
    // Use predefined device
    await using var browser = await Playwright.Chromium.LaunchAsync();
    await using var context = await browser.NewContextAsync(
        Playwright.Devices["iPhone 13"]);
    
    var page = await context.NewPageAsync();
    await page.GotoAsync("/customers");
    
    // Mobile-specific test
    await page.Locator(".mobile-menu-toggle").ClickAsync();
    await Expect(page.Locator(".mobile-menu")).ToBeVisibleAsync();
}

[Test]
public async Task CustomDeviceEmulation()
{
    await using var browser = await Playwright.Chromium.LaunchAsync();
    await using var context = await browser.NewContextAsync(new()
    {
        ViewportSize = new ViewportSize { Width = 390, Height = 844 },
        DeviceScaleFactor = 3,
        IsMobile = true,
        HasTouch = true,
        UserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 15_0 like Mac OS X)..."
    });
    
    var page = await context.NewPageAsync();
    await page.GotoAsync("/customers");
}

[Test]
public async Task TouchGesturesWork()
{
    // Mobile context
    await using var context = await Browser.NewContextAsync(
        Playwright.Devices["Pixel 5"]);
    var page = await context.NewPageAsync();
    
    await page.GotoAsync("/customers");
    
    // Swipe gesture
    var element = page.Locator(".swipeable-card").First;
    var box = await element.BoundingBoxAsync();
    
    await page.Mouse.MoveAsync(box!.X + box.Width / 2, box.Y + box.Height / 2);
    await page.Mouse.DownAsync();
    await page.Mouse.MoveAsync(box.X + 100, box.Y + box.Height / 2);
    await page.Mouse.UpAsync();
    
    // Verify swipe action
    await Expect(page.Locator(".delete-confirm")).ToBeVisibleAsync();
}
```

## Advanced Scenarios

### File Upload

```csharp
[Test]
public async Task CanUploadFile()
{
    await Page.GotoAsync("/customers/import");
    
    // Set input files
    var fileInput = Page.Locator("input[type='file']");
    await fileInput.SetInputFilesAsync("test-data/customers.csv");
    
    // Submit
    await Page.ClickAsync("button:has-text('Upload')");
    
    // Verify upload
    await Expect(Page.Locator("text=10 customers imported")).ToBeVisibleAsync();
}

[Test]
public async Task CanUploadMultipleFiles()
{
    await Page.GotoAsync("/documents/upload");
    
    var fileInput = Page.Locator("input[type='file']");
    await fileInput.SetInputFilesAsync(new[]
    {
        "file1.pdf",
        "file2.pdf"
    });
}
```

### File Download

```csharp
[Test]
public async Task CanDownloadReport()
{
    await Page.GotoAsync("/reports");
    
    // Start waiting for download before clicking
    var downloadTask = Page.WaitForDownloadAsync();
    await Page.ClickAsync("button:has-text('Download Report')");
    var download = await downloadTask;
    
    // Verify download
    Assert.That(download.SuggestedFilename, Is.EqualTo("customer-report.pdf"));
    
    // Save to disk
    await download.SaveAsAsync($"downloads/{download.SuggestedFilename}");
    
    // Verify file content
    var filePath = await download.PathAsync();
    var fileInfo = new FileInfo(filePath);
    Assert.That(fileInfo.Length, Is.GreaterThan(0));
}
```

### Working with Iframes

```csharp
[Test]
public async Task CanInteractWithIframe()
{
    await Page.GotoAsync("/embedded-form");
    
    // Get iframe
    var frame = Page.FrameLocator("iframe[name='payment-form']");
    
    // Interact with elements inside iframe
    await frame.Locator("[name='cardNumber']").FillAsync("4111111111111111");
    await frame.Locator("[name='expiry']").FillAsync("12/25");
    await frame.Locator("[name='cvc']").FillAsync("123");
    await frame.Locator("button[type='submit']").ClickAsync();
    
    // Assertions on iframe content
    await Expect(frame.Locator("text=Payment successful")).ToBeVisibleAsync();
}
```

### API Mocking and Network Interception

```csharp
[Test]
public async Task CanMockAPIResponses()
{
    // Mock API response
    await Page.RouteAsync("**/api/customers", async route =>
    {
        var json = JsonSerializer.Serialize(new[]
        {
            new { id = "1", name = "Mocked Customer", email = "mock@test.com" }
        });
        
        await route.FulfillAsync(new()
        {
            Status = 200,
            ContentType = "application/json",
            Body = json
        });
    });
    
    await Page.GotoAsync("/customers");
    
    // Should display mocked data
    await Expect(Page.Locator("text=Mocked Customer")).ToBeVisibleAsync();
}

[Test]
public async Task CanTestErrorHandling()
{
    // Simulate API error
    await Page.RouteAsync("**/api/customers", async route =>
    {
        await route.FulfillAsync(new()
        {
            Status = 500,
            ContentType = "application/json",
            Body = JsonSerializer.Serialize(new { error = "Internal server error" })
        });
    });
    
    await Page.GotoAsync("/customers");
    
    // Should show error message
    await Expect(Page.Locator(".alert-danger")).ToBeVisibleAsync();
}

[Test]
public async Task CanMonitorNetworkRequests()
{
    var requests = new List<IRequest>();
    
    // Monitor requests
    Page.Request += (_, request) =>
    {
        if (request.Url.Contains("/api/"))
        {
            requests.Add(request);
        }
    };
    
    await Page.GotoAsync("/customers");
    await Page.ClickAsync("button:has-text('Refresh')");
    
    // Verify requests were made
    Assert.That(requests.Count, Is.GreaterThan(0));
    Assert.That(requests.Any(r => r.Url.Contains("/api/customers")), Is.True);
}
```

## Test Organization and Fixtures

### Base Test Class

```csharp
public abstract class BaseTest : PageTest
{
    protected string BaseUrl => "https://localhost:5001";
    protected TestDataHelper TestData = null!;
    
    [SetUp]
    public async Task BaseSetup()
    {
        TestData = new TestDataHelper();
        await Page.GotoAsync(BaseUrl);
        
        // Set default timeout
        Page.SetDefaultTimeout(10000);
    }
    
    [TearDown]
    public async Task BaseTearDown()
    {
        // Cleanup test data
        await TestData.CleanupAsync();
        
        // Take screenshot on failure
        if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed)
        {
            var screenshot = await Page.ScreenshotAsync();
            TestContext.AddTestAttachment(
                $"{TestContext.CurrentContext.Test.Name}.png",
                screenshot);
        }
    }
}
```

### Test Data Management

```csharp
public class TestDataHelper
{
    private readonly HttpClient _httpClient = new();
    private readonly List<string> _createdCustomerIds = new();
    
    public async Task<CustomerDto> CreateTestCustomerAsync(string? name = null)
    {
        var customer = new
        {
            name = name ?? $"Test Customer {Guid.NewGuid()}",
            email = $"test-{Guid.NewGuid()}@example.com"
        };
        
        var response = await _httpClient.PostAsJsonAsync(
            "https://localhost:5001/api/customers",
            customer);
        
        var createdCustomer = await response.Content.ReadFromJsonAsync<CustomerDto>();
        _createdCustomerIds.Add(createdCustomer!.Id);
        
        return createdCustomer;
    }
    
    public async Task CleanupAsync()
    {
        foreach (var id in _createdCustomerIds)
        {
            await _httpClient.DeleteAsync($"https://localhost:5001/api/customers/{id}");
        }
        
        _createdCustomerIds.Clear();
    }
}
```

## CI/CD Integration

### Playwright Configuration

```csharp
// PlaywrightSetup.cs
[SetUpFixture]
public class PlaywrightSetup
{
    [OneTimeSetUp]
    public void InstallPlaywright()
    {
        // Install browsers (run once in CI)
        if (Environment.GetEnvironmentVariable("CI") == "true")
        {
            Microsoft.Playwright.Program.Main(new[] { "install", "chromium" });
        }
    }
}
```

### GitHub Actions Workflow

```yaml
name: E2E Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      
      - name: Install dependencies
        run: dotnet restore
      
      - name: Build
        run: dotnet build --no-restore
      
      - name: Install Playwright
        run: |
          dotnet tool install --global Microsoft.Playwright.CLI
          playwright install chromium
      
      - name: Run E2E tests
        run: dotnet test --no-build --verbosity normal
      
      - name: Upload test results
        if: always()
        uses: actions/upload-artifact@v3
        with:
          name: playwright-results
          path: test-results/
```

## Debugging Tips

### Headed Mode and Slow Mo

```csharp
// Launch browser in headed mode
await using var browser = await Playwright.Chromium.LaunchAsync(new()
{
    Headless = false,
    SlowMo = 1000 // Slow down by 1 second
});
```

### Debug with Playwright Inspector

```bash
# Set environment variable
PWDEBUG=1 dotnet test

# Or in code
Environment.SetEnvironmentVariable("PWDEBUG", "1");
```

### Tracing

```csharp
[Test]
public async Task TestWithTracing()
{
    // Start tracing
    await Context.Tracing.StartAsync(new()
    {
        Screenshots = true,
        Snapshots = true,
        Sources = true
    });
    
    try
    {
        await Page.GotoAsync("/customers");
        // Test steps...
    }
    finally
    {
        // Stop and save trace
        await Context.Tracing.StopAsync(new()
        {
            Path = "trace.zip"
        });
    }
}

// View trace: playwright show-trace trace.zip
```

## Best Practices

### 1. Reliable Selectors
- Prefer role-based and semantic selectors
- Use `data-testid` for dynamic elements
- Avoid CSS selectors that depend on styling

### 2. Reduce Flakiness
- Use auto-waiting assertions
- Avoid fixed `Task.Delay()` calls
- Use proper wait strategies
- Handle race conditions

### 3. Test Isolation
- Each test should be independent
- Clean up test data
- Don't rely on execution order

### 4. Performance
- Run tests in parallel
- Use authentication state
- Mock slow APIs when appropriate

### 5. Maintainability
- Use Page Object pattern
- Extract reusable helpers
- Keep tests focused and simple

## Resources

- [Playwright .NET Documentation](https://playwright.dev/dotnet/)
- [Playwright Best Practices](https://playwright.dev/docs/best-practices)
- [Deque Axe for Accessibility](https://www.deque.com/axe/playwright/)
- [Playwright Inspector](https://playwright.dev/docs/inspector)
