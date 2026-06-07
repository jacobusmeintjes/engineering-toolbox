namespace orleans_hive_shared;


public interface ICounterGrain : IGrainWithStringKey
{
    Task<int> Increment();
    Task<int> GetCount();
}
