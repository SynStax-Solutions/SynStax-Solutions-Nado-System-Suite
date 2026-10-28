namespace Nado_Graphics_MS_Backend.Services
{
    //Runs the overdue/expiry checks on a timer while the API is running.
    public class BillingStatusWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _config;
        private readonly ILogger<BillingStatusWorker> _logger;

        public BillingStatusWorker(
            IServiceScopeFactory scopeFactory,
            IConfiguration config,
            ILogger<BillingStatusWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _config = config;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var minutes = Math.Max(1, _config.GetValue("Billing:CheckIntervalMinutes", 60));
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(minutes));

            try
            {
                do
                {
                    await RunChecksAsync();
                }
                while (await timer.WaitForNextTickAsync(stoppingToken));
            }
            catch (OperationCanceledException)
            {
                //The API is shutting down.
            }
        }

        private async Task RunChecksAsync()
        {
            try
            {
                //Scoped services (DbContext, repositories) need their own scope in a background job.
                using var scope = _scopeFactory.CreateScope();
                var billing = scope.ServiceProvider.GetRequiredService<BillingStatusService>();

                var overdue = await billing.MarkOverdueInvoicesAsync();
                var expired = await billing.ExpireOutdatedQuotesAsync();

                if (overdue + expired > 0)
                {
                    _logger.LogInformation(
                        "Billing checks: {Overdue} invoice(s) marked overdue, {Expired} quote(s) expired.",
                        overdue, expired);
                }
            }
            catch (Exception ex)
            {
                //One failed run must not stop the worker.
                _logger.LogError(ex, "The billing status check failed.");
            }
        }
    }
}