using Bunit;
using BlazorRedux.Web.Components.Pages;
using BlazorRedux.Web.Features.State;
using BlazorRedux.Web.Features.Action;
using Fluxor;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BlazorRedux.Tests;

/// <summary>
/// Integration tests for the Counter component with Fluxor state management
/// </summary>
public class CounterIntegrationTests : TestContext
{
    public CounterIntegrationTests()
    {
        // Configure Fluxor with the same assembly scanning as the main app
        Services.AddFluxor(options =>
        {
            options.ScanAssemblies(typeof(Counter).Assembly);
        });
    }

    [Fact]
    public void Counter_InitialState_ShouldBeZero()
    {
        // Arrange
        var cut = RenderComponent<Counter>();

        // Act
        var countText = cut.Find("p[role='status']").TextContent;

        // Assert
        Assert.Contains("Current count: 0", countText);
    }

    [Fact]
    public void Counter_ClickIncrement_ShouldIncrementCount()
    {
        // Arrange
        var cut = RenderComponent<Counter>();
        var incrementButton = cut.Find("button.btn-primary");

        // Act
        incrementButton.Click();

        // Assert - Wait for state update
        cut.WaitForAssertion(() =>
        {
            var countText = cut.Find("p[role='status']").TextContent;
            Assert.Contains("Current count: 1", countText);
        }, timeout: TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Counter_ClickIncrementMultipleTimes_ShouldIncrementCountCorrectly()
    {
        // Arrange
        var cut = RenderComponent<Counter>();
        var incrementButton = cut.Find("button.btn-primary");

        // Act - Click increment 5 times
        for (int i = 0; i < 5; i++)
        {
            incrementButton.Click();
        }

        // Assert
        cut.WaitForAssertion(() =>
        {
            var countText = cut.Find("p[role='status']").TextContent;
            Assert.Contains("Current count: 5", countText);
        }, timeout: TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Counter_ClickDecrement_ShouldDecrementCount()
    {
        // Arrange
        var cut = RenderComponent<Counter>();
        var decrementButton = cut.Find("button.btn-secondary");

        // Act
        decrementButton.Click();

        // Assert
        cut.WaitForAssertion(() =>
        {
            var countText = cut.Find("p[role='status']").TextContent;
            Assert.Contains("Current count: -1", countText);
        }, timeout: TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Counter_IncrementAndDecrement_ShouldUpdateCountCorrectly()
    {
        // Arrange
        var cut = RenderComponent<Counter>();
        var incrementButton = cut.Find("button.btn-primary");
        var decrementButton = cut.Find("button.btn-secondary");

        // Act - Increment 3 times, then decrement 1 time
        incrementButton.Click();
        incrementButton.Click();
        incrementButton.Click();
        decrementButton.Click();

        // Assert - Should be 2
        cut.WaitForAssertion(() =>
        {
            var countText = cut.Find("p[role='status']").TextContent;
            Assert.Contains("Current count: 2", countText);
        }, timeout: TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Counter_LastUpdatedTimestamp_ShouldChangeAfterClick()
    {
        // Arrange
        var cut = RenderComponent<Counter>();
        var initialTimestamp = cut.FindAll("p small").FirstOrDefault()?.TextContent;
        var incrementButton = cut.Find("button.btn-primary");

        // Wait a bit to ensure timestamp will be different
        System.Threading.Thread.Sleep(100);

        // Act
        incrementButton.Click();

        // Assert
        cut.WaitForAssertion(() =>
        {
            var newTimestamp = cut.FindAll("p small").FirstOrDefault()?.TextContent;
            Assert.NotNull(newTimestamp);
            Assert.NotEqual(initialTimestamp, newTimestamp);
        }, timeout: TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Counter_FluxorStatus_ShouldBeConnected()
    {
        // Arrange & Act
        var cut = RenderComponent<Counter>();

        // Assert
        var statusText = cut.Find("div.mt-3 p").TextContent;
        Assert.Contains("✅ Connected", statusText);
    }

    [Fact]
    public void Counter_StateInjection_ShouldWorkCorrectly()
    {
        // Arrange
        var cut = RenderComponent<Counter>();

        // Act - Get the injected state from the component
        var state = Services.GetRequiredService<IState<CounterState>>();

        // Assert
        Assert.NotNull(state);
        Assert.NotNull(state.Value);
        Assert.Equal(0, state.Value.Count);
    }

    [Fact]
    public void Counter_DispatchAction_ShouldUpdateStateDirectly()
    {
        // Arrange
        var dispatcher = Services.GetRequiredService<IDispatcher>();
        var state = Services.GetRequiredService<IState<CounterState>>();

        // Act
        dispatcher.Dispatch(new IncrementCounterAction());

        // Wait for state update
        System.Threading.Thread.Sleep(100);

        // Assert
        Assert.Equal(1, state.Value.Count);
    }

    [Fact]
    public void Counter_MultipleDispatches_ShouldUpdateStateCorrectly()
    {
        // Arrange
        var dispatcher = Services.GetRequiredService<IDispatcher>();
        var state = Services.GetRequiredService<IState<CounterState>>();

        // Act
        dispatcher.Dispatch(new IncrementCounterAction());
        dispatcher.Dispatch(new IncrementCounterAction());
        dispatcher.Dispatch(new DecrementCounterAction());

        // Wait for state updates
        System.Threading.Thread.Sleep(100);

        // Assert
        Assert.Equal(1, state.Value.Count);
    }
}
