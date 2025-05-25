using System.Linq;
using Content.Goobstation.Shared.Xenobiology;
using Content.Goobstation.Shared.Xenobiology.Components;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Nyanotrasen.Item.PseudoItem;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Xenobiology;

/// <summary>
/// This handles any actions that slime mobs may have.
/// </summary>
public sealed class SlimeMobActionsSystem : EntitySystem
{

    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly HungerSystem _hunger = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedPseudoItemSystem _pseudoSystem = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;

    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlimeLatchEvent>(OnLatch);
        SubscribeLocalEvent<SlimeComponent, EntRemovedFromContainerMessage>(OnEntityEscape);
        SubscribeLocalEvent<SlimeDamageOvertimeComponent, MobStateChangedEvent>(OnEntityDied);
    }

    public override void Update(float frameTime)
    {
        var currentTime = _timing.CurTime;
        var query = EntityQueryEnumerator<SlimeDamageOvertimeComponent>();
        while (query.MoveNext(out var uid, out var dot))
        {
            if (currentTime < dot.NextTickTime
                || _mobState.IsDead(uid))
                continue;

            var addedHunger = (float)dot.Damage.GetTotal();
            dot.NextTickTime = currentTime + dot.Interval;
            _damageable.TryChangeDamage(uid, dot.Damage, ignoreResistances: true, ignoreBlockers: true,  targetPart: TargetBodyPart.All);

            if (TryComp<HungerComponent>(dot.SourceEntityUid, out var hunger))
                _hunger.ModifyHunger(dot.SourceEntityUid, addedHunger, hunger);
        }
    }

    #region Events
    private void OnLatch(SlimeLatchEvent args)
    {
        var slime = args.Performer;
        var target = args.Target;

        if (TerminatingOrDeleted(target)
            || TerminatingOrDeleted(slime))
            return;

        DoSlimeLatch(slime, target);
    }

    private void OnEntityDied(Entity<SlimeDamageOvertimeComponent> ent, ref MobStateChangedEvent args)
    {
        if (!HasComp<PseudoItemComponent>(ent))
            return;

        if (args.NewMobState == MobState.Dead)
            _container.TryRemoveFromContainer(ent, true);
    }

    private void OnEntityEscape(Entity<SlimeComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (!TryComp<PseudoItemComponent>(args.Entity, out var pseudoItem)
            || !HasComp<SlimeDamageOvertimeComponent>(args.Entity))
            return;

        RemCompDeferred<SlimeDamageOvertimeComponent>(args.Entity);

        if (!pseudoItem.IntendedComp)
            RemCompDeferred(args.Entity, pseudoItem);
    }
    #endregion

    #region Helpers
    private void DoSlimeLatch(EntityUid slime, EntityUid target)
    {
        if (_mobState.IsDead(target))
            return;

        var compExists = EntityManager.EnsureComponent<PseudoItemComponent>(target, out var item);

        if (!compExists)
            item.IntendedComp = false;

        var inserted = _pseudoSystem.TryInsert(slime, target, item);

        if (inserted)
        {
            EntityManager.EnsureComponent(target, out SlimeDamageOvertimeComponent comp);
            comp.SourceEntityUid = slime;
        }
        else
        {
            RemComp<PseudoItemComponent>(target);
        }
        //We also need to set a new state for the slime when it's consuming, this will be easy however it's important to take MobGrowthSystem into account... possibly we should use layers?
    }
    #endregion
}
