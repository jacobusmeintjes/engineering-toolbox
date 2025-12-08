using BlazorRedux.Web.Features.Action;
using BlazorRedux.Web.Features.State;
using Fluxor;
using Microsoft.Extensions.Logging;

namespace BlazorRedux.Web.Features.State
{
    public record CounterState
    {
        public int Count { get; init; }
        public DateTime LastUpdated { get; init; } = DateTime.Now;
    }
}

namespace BlazorRedux.Web.Features.Feature
{
    public class CounterFeature : Feature<CounterState>
    {
        public override string GetName() => "Counter";

        protected override CounterState GetInitialState()
        {
            return new CounterState { Count = 0 };
        }
    }
}

namespace BlazorRedux.Web.Features.Action
{
    public class IncrementCounterAction { }
    public class DecrementCounterAction { }
}

namespace BlazorRedux.Web.Features.Reducers
{
    public static class CounterReducers
    {
        [ReducerMethod]
        public static CounterState OnIncrement(CounterState state, IncrementCounterAction action)
        {
            System.Diagnostics.Debug.WriteLine($"[REDUCER] Increment: {state.Count} -> {state.Count + 1}");
            return state with { Count = state.Count + 1, LastUpdated = DateTime.Now };
        }

        [ReducerMethod]
        public static CounterState OnDecrement(CounterState state, DecrementCounterAction action)
        {
            System.Diagnostics.Debug.WriteLine($"[REDUCER] Decrement: {state.Count} -> {state.Count - 1}");
            return state with { Count = state.Count - 1, LastUpdated = DateTime.Now };
        }
    }
}

namespace BlazorRedux.Web.Features.Effects
{
    // Effects must be in their own NON-STATIC class
    public class CounterEffects
    {
        private readonly ILogger<CounterEffects> _logger;

        public CounterEffects(ILogger<CounterEffects> logger)
        {
            _logger = logger;
        }

        [EffectMethod]
        public Task HandleIncrement(IncrementCounterAction action, IDispatcher dispatcher)
        {
            _logger.LogInformation("[EFFECT] IncrementCounterAction triggered at {Time}", DateTime.Now);
            System.Diagnostics.Debug.WriteLine($"[EFFECT] IncrementCounterAction triggered at {DateTime.Now}");
            // Your effect logic here (e.g., API calls, side effects)
            return Task.CompletedTask;
        }

        [EffectMethod]
        public Task HandleDecrement(DecrementCounterAction action, IDispatcher dispatcher)
        {
            _logger.LogInformation("[EFFECT] DecrementCounterAction triggered at {Time}", DateTime.Now);
            System.Diagnostics.Debug.WriteLine($"[EFFECT] DecrementCounterAction triggered at {DateTime.Now}");
            return Task.CompletedTask;
        }
    }
}