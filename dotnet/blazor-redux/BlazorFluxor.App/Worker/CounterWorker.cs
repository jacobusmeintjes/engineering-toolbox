using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace BlazorFluxor.App.Worker
{
    public class CounterWorker : BackgroundService
    {
        private readonly CounterService _counterService;

        public CounterWorker(CounterService counterService)
        {
            _counterService = counterService;
        }



        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(10_000, cancellationToken);

            while (!cancellationToken.IsCancellationRequested)
            {
                _counterService.UpdateCounter(1);
                await Task.Delay(2000, cancellationToken);
            }
        }
    }


    public class CounterService
    {
        private readonly Subject<int> _counterUpdates = new();
        public IObservable<int> CounterUpdates => _counterUpdates.AsObservable();

        public void UpdateCounter(int count)
        {
            Console.WriteLine("FIRE IN THE HOLE!!!!!");
            _counterUpdates.OnNext(count);
        }
    }
}
