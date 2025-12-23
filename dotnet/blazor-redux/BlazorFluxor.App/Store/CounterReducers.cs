using Fluxor;

namespace BlazorFluxor.App.Store
{
    // Reducers/CounterReducers.cs
    public static class Reducers
    {
        [ReducerMethod]
        public static CounterState OnIncrement(CounterState state, IncrementCounterAction _) {
            Console.WriteLine("Reducer fired!");
            //state.Count = state.Count + 1;
            return state with { Count = state.Count + 1 };
        }

    }
}
