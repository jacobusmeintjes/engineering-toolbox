

using orleans_hive_shared;

public class CounterGrain : Grain, ICounterGrain
{

    private readonly IPersistentState<CounterState> _state;
    public CounterGrain([PersistentState("counter", "Default")] IPersistentState<CounterState> state) => _state = state;

    public async Task<int> Increment()
    {
        _state.State.Count++;

        await _state.WriteStateAsync();

        return _state.State.Count;
    }
    public Task<int> GetCount()
    {
        return Task.FromResult(_state.State.Count);
    }
}