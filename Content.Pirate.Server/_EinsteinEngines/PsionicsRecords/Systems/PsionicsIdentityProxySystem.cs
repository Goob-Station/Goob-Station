using Content.Shared.IdentityManagement;

namespace Content.Server.PsionicsRecords.Systems;

/// <summary>
/// Proxy system to handle psionics records identity updates
/// This replaces the direct dependency from Content.Server.IdentitySystem
/// </summary>
public sealed class PsionicsIdentityProxySystem : EntitySystem
{
    [Dependency] private readonly PsionicsRecordsConsoleSystem _psionicsRecordsConsole = default!;

    public override void Initialize()
    {
        base.Initialize();

        // Subscribe to identity changed events to update psionics records
        SubscribeLocalEvent<IdentityChangedEvent>(OnIdentityChanged);
    }

    private void OnIdentityChanged(ref IdentityChangedEvent args)
    {
        // Update psionics records when identity changes
        _psionicsRecordsConsole.CheckNewIdentity(args.CharacterEntity);
    }
}
