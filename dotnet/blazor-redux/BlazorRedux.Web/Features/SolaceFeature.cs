using BlazorRedux.Web.Features.Solace.Action;
using BlazorRedux.Web.Features.Solace.State;
using Fluxor;

namespace BlazorRedux.Web.Features.Solace.State
{
    public record SolaceState
    {
        public int Count { get; init; }
        public DateTime LastUpdated { get; init; } = DateTime.Now;
    }
}

namespace BlazorRedux.Web.Features.Solace.Feature
{
    public class SolaceFeature : Feature<SolaceState>
    {
        public override string GetName() => "Solace";

        protected override SolaceState GetInitialState()
        {
            return new SolaceState { Count = 0 };
        }
    }
}

namespace BlazorRedux.Web.Features.Solace.Action
{
    public class IncrementCounterAction { }
    public class DecrementCounterAction { }
}

namespace BlazorRedux.Web.Features.Solace.Reducers
{
    public static class SolaceReducers
    {
        [ReducerMethod]
        public static SolaceState OnIncrement(SolaceState state, IncrementCounterAction action)
        {
            System.Diagnostics.Debug.WriteLine($"[REDUCER] Increment: {state.Count} -> {state.Count + 1}");
            return state with { Count = state.Count + 1, LastUpdated = DateTime.Now };
        }

        [ReducerMethod]
        public static SolaceState OnDecrement(SolaceState state, DecrementCounterAction action)
        {
            System.Diagnostics.Debug.WriteLine($"[REDUCER] Decrement: {state.Count} -> {state.Count - 1}");
            return state with { Count = state.Count - 1, LastUpdated = DateTime.Now };
        }
    }
}

namespace BlazorRedux.Web.Features.Solace.Effects
{
    // Effects must be in their own NON-STATIedgitC class
    public class SolaceEffects
    {
        private readonly ILogger<SolaceEffects> _logger;

        public SolaceEffects(ILogger<SolaceEffects> logger)
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