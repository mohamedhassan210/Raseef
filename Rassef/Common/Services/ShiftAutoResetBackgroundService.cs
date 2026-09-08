namespace Rassef.Common.Services
{
    public class ShiftAutoResetBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ShiftAutoResetBackgroundService> _logger;
        private HashSet<int> _previouslyActiveShiftIds = new();
        private bool _initialized = false;

        public ShiftAutoResetBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<ShiftAutoResetBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckShiftsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while checking for shift auto-reset.");
                }

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }

        private async Task CheckShiftsAsync()
        {
            // Scoped repositories can't be injected into this singleton-lifetime
            // service's constructor — resolved fresh from a new scope on every tick.
            using var scope = _scopeFactory.CreateScope();
            var shiftRepository = scope.ServiceProvider.GetRequiredService<IRepository<Shift>>();
            var queueSettingsRepository = scope.ServiceProvider.GetRequiredService<IRepository<QueueSettings>>();

            var settings = (await queueSettingsRepository.GetAllAsync()).FirstOrDefault();

            // Only acts under ByShift mode — firing this while the admin has chosen
            // Daily or Manual would override a reset strategy they explicitly picked.
            if (settings == null || settings.ResetType != ResetType.ByShift)
            {
                _initialized = false;
                return;
            }

            var allShifts = (await shiftRepository.GetAllAsync()).ToList();
            var currentTime = DateTimeOffset.Now.TimeOfDay;

            bool IsWithinWindow(Shift s)
            {
                var start = s.StartTime;
                var end = start.Add(s.Duration);
                if (end.TotalHours <= 24)
                {
                    return currentTime >= start && currentTime < end;
                }
                else
                {
                    var endNextDay = end.Subtract(TimeSpan.FromHours(24));
                    return currentTime >= start || currentTime < endNextDay;
                }
            }

            var currentlyActiveIds = allShifts.Where(IsWithinWindow).Select(s => s.Id).ToHashSet();

            if (!_initialized)
            {
                // First check since the app started (or since switching into ByShift
                // mode) — just record the current state. Don't fire a reset for a
                // shift that may have already been inactive before this ever started
                // watching.
                _previouslyActiveShiftIds = currentlyActiveIds;
                _initialized = true;
                return;
            }

            // A shift that was active on the last check and isn't anymore just ended.
            var justEndedIds = _previouslyActiveShiftIds.Except(currentlyActiveIds).ToList();

            if (justEndedIds.Any())
            {
                var now = DateTimeOffset.Now;
                foreach (var shift in allShifts.Where(s => justEndedIds.Contains(s.Id)))
                {
                    // Same effect as clicking "تصفير" on Shift/Index — no confirm
                    // popup, because there's no user present to confirm.
                    shift.LastResetAt = now;
                    shift.MarkAsUpdated();
                    shiftRepository.Update(shift);
                    _logger.LogInformation("Auto-reset shift '{ShiftName}' (Id {ShiftId}) — window ended.", shift.Name, shift.Id);
                }
                await shiftRepository.SaveChangesAsync();
            }

            _previouslyActiveShiftIds = currentlyActiveIds;
        }
    }
}