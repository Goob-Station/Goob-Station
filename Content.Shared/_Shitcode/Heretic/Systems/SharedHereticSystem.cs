using System.Diagnostics.CodeAnalysis;
using Content.Goobstation.Common.Heretic;
using Content.Shared.Heretic;
using Content.Shared.Mind;

namespace Content.Shared._Shitcode.Heretic.Systems;

public abstract class SharedHereticSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;

    private EntityQuery<HereticComponent> _hereticQuery;
    private EntityQuery<GhoulComponent> _ghoulQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HereticCheckEvent>(OnCheck);

        _hereticQuery = GetEntityQuery<HereticComponent>();
        _ghoulQuery = GetEntityQuery<GhoulComponent>();
    }

    private void OnCheck(ref HereticCheckEvent ev)
    {
        ev.Result = TryGetHereticComponent(ev.Uid, out _, out _);
    }

    public bool TryGetHereticComponent(
        EntityUid uid,
        [NotNullWhen(true)] out HereticComponent? heretic,
        out EntityUid mind)
    {
        heretic = null;
        return _mind.TryGetMind(uid, out mind, out _) && _hereticQuery.TryComp(mind, out heretic);
    }

    public bool IsHereticOrGhoul(EntityUid uid)
    {
        return _ghoulQuery.HasComp(uid) || TryGetHereticComponent(uid, out _, out _);
    }
}
