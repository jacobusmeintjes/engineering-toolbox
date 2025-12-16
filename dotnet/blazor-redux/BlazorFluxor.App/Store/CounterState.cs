using Fluxor;

namespace BlazorFluxor.App.Store
{

    [FeatureState]
    public record CounterState
    {
        public int Count { get; init; }

        
        public CounterState()
        {
            Count = 0;
        }
    }
}
