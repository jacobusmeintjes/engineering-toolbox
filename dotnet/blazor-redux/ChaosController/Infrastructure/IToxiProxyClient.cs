using Refit;
using System;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ChaosController.Infrastructure
{
    public interface IToxiProxyClient
    {
        [Post("/proxies")]
        Task Add(Proxy proxy);


        [Get("/proxies")]
        Task<Dictionary<string, Proxy>> List();


        [Get("/proxies/{proxy}")]
        Task<Proxy> Get(string proxy);

        [Post("/proxies/{proxy}")]
        Task Update(Proxy proxy);

        [Delete("/proxies/{proxy}")]
        Task Delete(Proxy proxy);

        [Post("/reset")]
        Task Reset();

        [Post("/version")]
        Task Version();

        /*
         
         POST /populate - Create or replace a list of proxies
GET /proxies/{proxy}/toxics - List active toxics
POST /proxies/{proxy}/toxics - Create a new toxic
GET /proxies/{proxy}/toxics/{toxic} - Get an active toxic's fields
POST /proxies/{proxy}/toxics/{toxic} - Update an active toxic
DELETE /proxies/{proxy}/toxics/{toxic} - Remove an active toxic

         */
    }

    public record Proxy(string Name, string Listen, string Upstream, bool Enabled);
}
