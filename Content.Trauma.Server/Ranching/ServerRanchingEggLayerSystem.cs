using Content.Server.Popups;
using Content.Shared.Mobs.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Trauma.Shared.Ranching.Components;
using Content.Trauma.Shared.Ranching.Events;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Trauma.Server.Ranching;

/// <summary>
/// This handles raising the egg layer event on the chicken when it should lay an egg.
/// </summary>
public sealed class ServerRanchingEggLayerSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly HungerSystem _hunger = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<RanchingEggLayerComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<RanchingEggLayerComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextGrowth = _timing.CurTime + TimeSpan.FromSeconds(_random.NextFloat(ent.Comp.EggLayCooldownMin, ent.Comp.EggLayCooldownMax));
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var toLayEgg = new List<(EntityUid uid, RanchingEggLayerComponent comp)>();

        var query = EntityQueryEnumerator<RanchingEggLayerComponent>();
        while (query.MoveNext(out var uid, out var eggLayer))
        {
            if (_mobState.IsDead(uid) || _mobState.IsCritical(uid))
                continue;

            if (eggLayer.HungerRequired && !HasComp<HungerComponent>(uid))
                continue;

            if (_timing.CurTime < eggLayer.NextGrowth)
                continue;

            eggLayer.NextGrowth += TimeSpan.FromSeconds(_random.NextFloat(eggLayer.EggLayCooldownMin, eggLayer.EggLayCooldownMax));

            toLayEgg.Add((uid, eggLayer));
        }

        foreach (var (uid, eggLayer) in toLayEgg)
        {
            TryLayEgg(uid, eggLayer);
        }
    }

    public void TryLayEgg(EntityUid uid, RanchingEggLayerComponent? egglayer)
    {
        if (!Resolve(uid, ref egglayer))
            return;

        if (!TryComp<HungerComponent>(uid, out var hunger))
            return;

        if (_hunger.GetHunger(hunger) < egglayer.HungerUsage)
        {
            _popup.PopupEntity(Loc.GetString("action-popup-lay-egg-too-hungry"), uid, uid);
            return;
        }

        var evfood = new RanchingEggLayAttemptEvent((uid, egglayer));
        RaiseLocalEvent(uid, ref evfood);
    }
}
