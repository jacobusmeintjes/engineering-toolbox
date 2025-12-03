

namespace Orleans.Caching.Contracts.Grains
{
    public interface ICacheGrain : IGrainWithStringKey
    {
        Task Set(string value, TimeSpan? ttl = null);
        Task<string> Get();
    }
}
