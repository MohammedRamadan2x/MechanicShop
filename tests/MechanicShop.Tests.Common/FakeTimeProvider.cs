namespace MechanicShop.Tests.Common;

public sealed class FakeTimeProvider : TimeProvider
{
    private DateTimeOffset _utcNow;

    public void SetUtcNow(DateTimeOffset utcNow)
    {
        _utcNow = utcNow;
    }

    public override DateTimeOffset GetUtcNow() => _utcNow;

    public override long GetTimestamp() => _utcNow.ToUnixTimeMilliseconds();
}