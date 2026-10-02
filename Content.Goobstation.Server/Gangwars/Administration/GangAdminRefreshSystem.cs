using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Gangwars.Administration;

/// <summary>
/// Keeps any open GangAdminEui panels in sync with gang data that changes outside the panel
/// (points earned from spray cans/crates, members joining or being kicked through gameplay, etc.)
/// </summary>
public sealed class GangAdminRefreshSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    private static readonly TimeSpan RefreshInterval = TimeSpan.FromSeconds(1);

    private readonly HashSet<GangAdminEui> _open = new();
    private TimeSpan _nextRefresh;

    public void Register(GangAdminEui eui) => _open.Add(eui);

    public void Unregister(GangAdminEui eui) => _open.Remove(eui);

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_open.Count == 0 || _timing.CurTime < _nextRefresh)
            return;

        _nextRefresh = _timing.CurTime + RefreshInterval;

        foreach (var eui in _open)
            eui.StateDirty();
    }
}
