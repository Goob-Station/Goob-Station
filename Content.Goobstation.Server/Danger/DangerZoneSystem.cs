using Content.Goobstation.Shared.Danger;
using Content.Shared.Popups;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Danger;

/// <summary>
/// Shows a pop-up to all players nearby on a timer.
/// Picks randomly which pop-up to show.
/// </summary>
public sealed partial class DangerZoneSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;

    private readonly HashSet<Entity<ActorComponent>> _mobCache = new();

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<DangerZoneComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime < comp.NextPopup || comp.Popups.Count == 0)
                continue;

            comp.NextPopup = _timing.CurTime + comp.PopupInterval;

            TryPopup(uid, comp);
        }
    }

    private void TryPopup(EntityUid uid, DangerZoneComponent comp)
    {
        var message = Loc.GetString(_random.Pick(comp.Popups));

        _mobCache.Clear();
        _lookup.GetEntitiesInRange(Transform(uid).Coordinates, comp.PopupRange, _mobCache);

        foreach (var mob in _mobCache)
        {
            if (mob.Owner == uid)
                continue;

            _popup.PopupEntity(message, mob, mob, PopupType.MediumCaution);
        }
    }
}
