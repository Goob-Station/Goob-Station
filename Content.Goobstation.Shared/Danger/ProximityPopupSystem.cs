using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Random.Helpers;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Danger;

/// <summary>
/// Shows a pop-up to all players nearby on a timer.
/// Picks randomly which pop-up to show.
/// </summary>
public sealed partial class ProximityPopupSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;

    private readonly HashSet<Entity<MobStateComponent>> _mobCache = new();

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ProximityPopupComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime < comp.NextPopup || comp.Popups.Count == 0)
                continue;

            var rand = new RobustRandom();
            rand.SetSeed(SharedRandomExtensions.HashCodeCombine((int) _timing.CurTick.Value, GetNetEntity(uid).Id));

            // As per the component statement, picks PopupInterval as default, but if a MaxPopupInterval is set, picks a random number in-between.
            var interval = comp.MaxPopupInterval is null ? comp.PopupInterval : rand.Next(comp.PopupInterval, comp.MaxPopupInterval.Value);
            comp.NextPopup = _timing.CurTime + interval;

            Dirty(uid, comp);

            TryPopup((uid, comp), rand);
        }
    }

    private void TryPopup(Entity<ProximityPopupComponent> ent, IRobustRandom rand)
    {
        var message = Loc.GetString(rand.Pick(ent.Comp.Popups));

        _mobCache.Clear();
        _lookup.GetEntitiesInRange(Transform(ent.Owner).Coordinates, ent.Comp.PopupRange, _mobCache);

        foreach (var mob in _mobCache)
        {
            if (mob.Owner == ent.Owner)
                continue;

            _popup.PopupClient(message, mob, mob, ent.Comp.PopupType);
        }
    }
}
