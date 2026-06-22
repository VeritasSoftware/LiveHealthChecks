namespace LiveHealthChecks.UI.Services
{
    public interface ISystemStateService
    {
        int IntervalMs { get; set; }
        int SleepThresholdMs { get; set; }

        event Func<Task>? SystemResumed;

        ValueTask DisposeAsync();
        void Start();
    }

    public class SystemStateService : ISystemStateService, IAsyncDisposable
    {
        public int IntervalMs { get; set; } = 1000 * 10;

        public int SleepThresholdMs { get; set; } = 1000 * 20;

        public event Func<Task>? SystemResumed;

        private readonly PeriodicTimer _timer;
        private CancellationTokenSource _cts;
        private Task _taskLoop = null!;

        private static DateTime _last = DateTime.UtcNow;

        public SystemStateService()
        {
            Console.WriteLine($"Starting background timer...");
            // Start the periodic task
            _timer = new PeriodicTimer(TimeSpan.FromMilliseconds(IntervalMs));
            _cts = new CancellationTokenSource();

            Console.WriteLine($"Started background timer...");
        }

        public void Start()
        {
            if (_taskLoop != null)
                return; // avoid multiple starts

            _taskLoop = Task.Run(async () =>
            {
                try
                {
                    while (await _timer.WaitForNextTickAsync(_cts.Token))
                    {
                        await DoWorkAsync();
                    }
                }
                catch (OperationCanceledException)
                {
                    // Normal shutdown
                }
            });
        }

        private async Task DoWorkAsync()
        {
            try
            {
                var now = DateTime.UtcNow;
                var diff = (now - _last).TotalMilliseconds;

                // If the difference is much larger than expected, system was suspended
                if (diff > (IntervalMs + SleepThresholdMs))
                {
                    Console.WriteLine($"Firing system resumed event...");
                    // Fire system resumed event
                    SystemResumed?.Invoke();

                    Console.WriteLine($"Finished system resumed event...");
                }

                _last = now;
            }
            catch
            {
                // Prevent the timer from crashing on exceptions
            }
        }

        public async ValueTask DisposeAsync()
        {
            _cts.Cancel();

            if (_taskLoop != null)
                await _taskLoop;

            _cts.Dispose();
        }
    }
}