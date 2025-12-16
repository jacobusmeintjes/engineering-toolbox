//namespace BlazorFluxor.App.Store
//{
//    using Fluxor;
//    using System.Text.Json;

//    public class LoggingMiddleware : Middleware
//    {
//        private IStore _store;

//        public override Task InitializeAsync(IDispatcher dispatcher, IStore store)
//        {
//            _store = store;
//            Console.WriteLine("=== Fluxor Store Initialized ===");
//            return Task.CompletedTask;
//        }

//        public override bool MayDispatchAction(object action)
//        {
//            Console.WriteLine($"[MIDDLEWARE] MayDispatchAction: {action.GetType().Name}");
//            return true; // Return false to block the action
//        }

//        public override void BeforeDispatch(object action)
//        {
//            Console.WriteLine($"[MIDDLEWARE] BeforeDispatch: {action.GetType().Name}");
//            Console.WriteLine($"[MIDDLEWARE] Store initialized: {_store?.Initialized}");
//        }

//        public override void AfterDispatch(object action)
//        {
//            Console.WriteLine($"[MIDDLEWARE] AfterDispatch: {action.GetType().Name}");
//        }
//    }


//    public class DebugMiddleware : Middleware
//    {
//        private IStore _store;
//        private readonly ILogger<DebugMiddleware> _logger;

//        public DebugMiddleware(ILogger<DebugMiddleware> logger)
//        {
//            _logger = logger;
//        }

//        public override Task InitializeAsync(IDispatcher dispatcher, IStore store)
//        {
//            _store = store;

//            _logger.LogInformation("=== FLUXOR STORE INITIALIZED ===");
//            _logger.LogInformation($"Features found: {store.Features.Count()}");

//            foreach (var feature in store.Features)
//            {
//                _logger.LogInformation($"  - Feature: {feature.Value.GetName()}");
//            }

//            return Task.CompletedTask;
//        }

//        public override bool MayDispatchAction(object action)
//        {
//            _logger.LogDebug($"[PRE-CHECK] Action: {action.GetType().Name}");
//            return true;
//        }

//        public override void BeforeDispatch(object action)
//        {
//            _logger.LogInformation("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
//            _logger.LogInformation($"[BEFORE] Action: {action.GetType().Name}");
//            _logger.LogInformation($"[BEFORE] Store Initialized: {_store?.Initialized}");

//            // Log current state before action
//            foreach (var feature in _store.Features)
//            {
//                var state = feature.Value.GetState();
//                _logger.LogInformation($"[BEFORE] {feature.Value.GetName()} State: {JsonSerializer.Serialize(state)}");
//            }
//        }

//        public override void AfterDispatch(object action)
//        {
//            _logger.LogInformation($"[AFTER] Action: {action.GetType().Name}");

//            // Log state after action
//            foreach (var feature in _store.Features)
//            {
//                var state = feature.Value.GetState();
//                _logger.LogInformation($"[AFTER] {feature.Value.GetName()} State: {JsonSerializer.Serialize(state)}");
//            }

//            _logger.LogInformation("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
//        }
//    }

//    public class ReducerDiscoveryMiddleware : Middleware
//    {
//        private readonly ILogger<ReducerDiscoveryMiddleware> _logger;

//        public ReducerDiscoveryMiddleware(ILogger<ReducerDiscoveryMiddleware> logger)
//        {
//            _logger = logger;
//        }

//        public override Task InitializeAsync(IDispatcher dispatcher, IStore store)
//        {
//            _logger.LogInformation("=== CHECKING REDUCER REGISTRATION ===");

//            foreach (var feature in store.Features)
//            {
//                var featureType = feature.GetType();
//                _logger.LogInformation($"Feature: {feature.Value.GetName()}");
//                _logger.LogInformation($"  Type: {featureType.FullName}");
//                _logger.LogInformation($"  State Type: {feature.Value.GetStateType().FullName}");

//                // Try to get reducer info through reflection
//                var reducersField = featureType.BaseType?
//                    .GetField("_reducers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

//                if (reducersField != null)
//                {
//                    var reducers = reducersField.GetValue(feature) as System.Collections.IEnumerable;
//                    if (reducers != null)
//                    {
//                        var count = 0;
//                        foreach (var reducer in reducers)
//                        {
//                            count++;
//                            _logger.LogInformation($"  Reducer {count}: {reducer.GetType().Name}");
//                        }
//                        _logger.LogInformation($"  Total reducers: {count}");
//                    }
//                    else
//                    {
//                        _logger.LogWarning($"  NO REDUCERS FOUND!");
//                    }
//                }
//            }

//            return Task.CompletedTask;
//        }
//    }
//}
