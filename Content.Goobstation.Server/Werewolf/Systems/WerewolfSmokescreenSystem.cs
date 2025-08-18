using Content.Goobstation.Shared.Werewolf.Components;
using Content.Goobstation.Shared.Werewolf.Events;
using Content.Server.Fluids.EntitySystems;
using Content.Server.Spreader;
using Content.Server.Stealth;
using Content.Shared.Chemistry.Components;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.Maps;
using Content.Shared.StatusEffect;
using Content.Shared.Stealth.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Werewolf.Systems;

/// <summary>
/// This handles the smokescreen ability.
/// Smokescreen turns you fully invisible and makes a smoke cloud where you stand.
/// However, you are unable to attack in this state.
/// </summary>
public sealed class WerewolfSmokescreenSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IMapManager _mapMan = default!;
    [Dependency] private readonly SmokeSystem _smoke = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly SpreaderSystem _spreader = default!;
    [Dependency] private  readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly StealthSystem _stealth = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<WerewolfSmokescreenComponent, WerewolfSmokescreenlEvent>(OnSmokescreen);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var goidaEqe = EntityQueryEnumerator<WerewolfSmokescreenComponent>();
        while (goidaEqe.MoveNext(out var uid, out var comp))
        {
            if (!comp.Active)
                continue;

            if (_timing.CurTime < comp.NextUpdate)
                continue;

            comp.Active = false;
            RemComp<StealthComponent>(uid);
        }
    }

    private void OnSmokescreen(Entity<WerewolfSmokescreenComponent> ent, ref WerewolfSmokescreenlEvent args)
    {
        StartTheSmoke(ent.Owner, ent.Comp);

        // Adjust timers
        ent.Comp.Active = true;
        ent.Comp.NextUpdate = _timing.CurTime + ent.Comp.EffectDuration;

        // Apply effects
        _statusEffects.TryAddStatusEffect<PacifiedComponent>(ent.Owner, ent.Comp.Pacified, ent.Comp.EffectDuration, true);

        EnsureComp<StealthComponent>(ent.Owner);
        _stealth.SetVisibility(ent.Owner, -1); // -1 = fully invisible
    }

    public void StartTheSmoke(EntityUid uid, WerewolfSmokescreenComponent comp)
    {
        // smokeontriggerassgoida65supercode
        var xform = Transform(uid);
        var mapCoords = _transform.GetMapCoordinates(uid, xform);

        if (!_mapMan.TryFindGridAt(mapCoords, out _, out var grid)
            || !grid.TryGetTileRef(xform.Coordinates, out var tileRef)
            || tileRef.Tile.IsEmpty)
            return;

        if (_spreader.RequiresFloorToSpread(comp.SmokePrototype.ToString()) && tileRef.Tile.IsSpace())
            return;

        var coords = grid.MapToGrid(mapCoords);
        var entity = Spawn(comp.SmokePrototype, coords.SnapToGrid());

        if (!TryComp<SmokeComponent>(entity, out var smoke))
        {
            Log.Error($"Smoke prototype {comp.SmokePrototype} was missing SmokeComponent");
            Del(entity);
            return;
        }

        _smoke.StartSmoke(entity, comp.Solution, comp.Duration, comp.SpreadAmount, smoke);
    }
}
