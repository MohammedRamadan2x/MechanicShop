namespace MechanicShop.Infrastructure.Settings;

public class AppSettings
{
    public TimeOnly OpeningTime { get; set; }
    public TimeOnly ClosingTime { get; set; }
    public int MaxSpots { get; set; }
    public int MinimumAppointmentDurationInMinutes { get; set; }
    public int LocalCacheExpirationInMins { get; set; }
    public int DistributedCacheExpirationMins { get; set; }
    public int DefaultPageNumber { get; set; }
    public int DefaultPageSize { get; set; }
    public int BookingCancellationThresholdMinutes { get; set; }
    public int OverdueBookingCleanupFrequencyMinutes { get; set; }
    public string CorsPolicyName { get; set; } = default!;
    public string[] AllowedOrigins { get; set; } = default!;
}