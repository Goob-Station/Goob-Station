namespace Content.Server.PsionicsRecords.Systems;

/// <summary>
/// Stub/proxy class to satisfy Content.Server.IdentitySystem dependency
/// The actual implementation is in Content.Pirate.Server
/// This is placed in Common so Core modules can reference it
/// </summary>
public sealed class PsionicsRecordsConsoleSystem : EntitySystem
{
    /// <summary>
    /// Stub method - actual implementation is handled by PsionicsIdentityProxySystem
    /// </summary>
    public void CheckNewIdentity(EntityUid uid)
    {
        // This is intentionally empty - the actual logic is handled by
        // PsionicsIdentityProxySystem in Content.Pirate.Server through events
    }
}
