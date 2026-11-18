namespace Content.Shared.Trigger.Systems;

public sealed partial class TriggerSystem
{
    private static readonly TimeSpan CancelledProximityRetry = TimeSpan.FromSeconds(0.25);
}
