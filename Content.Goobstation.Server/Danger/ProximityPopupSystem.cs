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
public sealed partial class ProximityPopupSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;

    private readonly HashSet<Entity<ActorComponent>> _mobCache = new();

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ProximityPopupComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime < comp.NextPopup || comp.Popups.Count == 0)
                continue;

            // As per the component statement, picks PopupInterval as default, but if a MaxPopupInterval is set, picks a random number in-between.
            var interval = comp.MaxPopupInterval is null ? comp.PopupInterval : _random.Next(comp.PopupInterval, comp.MaxPopupInterval.Value);
            comp.NextPopup = _timing.CurTime + interval;

            TryPopup((uid,comp));
        }
    }

    private void TryPopup(Entity<ProximityPopupComponent> ent)
    {
        var message = Loc.GetString(_random.Pick(ent.Comp.Popups));

        _mobCache.Clear();
        _lookup.GetEntitiesInRange(Transform(ent.Owner).Coordinates, ent.Comp.PopupRange, _mobCache);

        foreach (var mob in _mobCache)
        {
            if (mob.Owner == ent.Owner)
                continue;

            _popup.PopupEntity(message, mob, mob, ent.Comp.PopupType);
        }
    }
}
