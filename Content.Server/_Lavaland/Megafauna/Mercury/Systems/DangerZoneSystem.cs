using Content.Shared._Lavaland.Megafauna.Mercury.Components;
using Content.Shared._Lavaland.MobPhases;
using Content.Shared.Chat;
using Content.Shared.Mobs.Components;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Robust.Shared.Map;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Lavaland.Megafauna.Mercury.Systems;

/// <summary>
/// Shows a pop-up to all players nearby on a timer.
/// Picks randomly which pop-up to show.
/// </summary>
public sealed class DangerZoneSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;

    private readonly HashSet<Entity<ActorComponent>> _mobCache = new();
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<DangerZoneComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime < comp.Accumulator)
                continue;

            comp.Accumulator = _timing.CurTime + comp.Interval;

            TryPopup(uid, comp);
        }
    }

    private void TryPopup(EntityUid uid, DangerZoneComponent comp)
    {

        var popup = _random.Pick(comp.Popup);

        var centerCoords = Transform(uid).Coordinates;

        _mobCache.Clear();
        _lookup.GetEntitiesInRange(centerCoords, comp.PopUpRange, _mobCache);

        foreach (var mob in _mobCache)
        {
            if (mob.Owner == uid)
                continue;

            _popup.PopupEntity(Loc.GetString(popup), mob, mob, PopupType.MediumCaution);
        }
    }
}
