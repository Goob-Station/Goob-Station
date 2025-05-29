using Content.Goobstation.Shared.Xenobiology;
using Content.Goobstation.Shared.Xenobiology.Components;
using Content.Server.Popups;
using Content.Server.Stunnable;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.ActionBlocker;
using Content.Shared.Damage;
using Content.Shared.Humanoid;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Nyanotrasen.Item.PseudoItem;
using Content.Shared.Storage;
using Robust.Server.Audio;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Xenobiology;

/// <summary>
/// This handles any actions that slime mobs may have.
/// </summary>
public sealed class SlimeMobActionsSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly HungerSystem _hunger = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedPseudoItemSystem _pseudoSystem = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlimeLatchEvent>(OnLatch);
        SubscribeLocalEvent<SlimeComponent, EntRemovedFromContainerMessage>(OnEntityEscape);
        SubscribeLocalEvent<SlimeComponent, MobStateChangedEvent>(OnEntityDied);
        SubscribeLocalEvent<SlimeDamageOvertimeComponent, MobStateChangedEvent>(OnConsumedEntityDied);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<SlimeDamageOvertimeComponent>();
        while (query.MoveNext(out var uid, out var dotComp))
        {
            if (_timing.CurTime < dotComp.NextTickTime
                || _mobState.IsDead(uid))
                continue;

            var addedHunger = (float)dotComp.Damage.GetTotal();
            dotComp.NextTickTime = _timing.CurTime + dotComp.Interval;
            _damageable.TryChangeDamage(uid, dotComp.Damage, ignoreResistances: true, ignoreBlockers: true,  targetPart: TargetBodyPart.All);

            if (TryComp<HungerComponent>(dotComp.SourceEntityUid, out var hunger) &&
                dotComp.SourceEntityUid is { } sourceEntity)
            {
                _hunger.ModifyHunger(sourceEntity, addedHunger, hunger);
                Dirty(sourceEntity, hunger);
            }
        }
    }

    #region Events
    private void OnLatch(SlimeLatchEvent args)
    {
        var slime = args.Performer;
        var target = args.Target;

        if (TerminatingOrDeleted(target)
            || TerminatingOrDeleted(slime)
            || !TryComp<SlimeComponent>(slime, out var slimeComp))
            return;

        DoSlimeLatch(slime, target, slimeComp);
    }

    private void OnConsumedEntityDied(Entity<SlimeDamageOvertimeComponent> ent, ref MobStateChangedEvent args)
    {
        if (HasComp<PseudoItemComponent>(ent) && args.NewMobState == MobState.Dead)
            _container.TryRemoveFromContainer(ent, true);
    }

    private void OnEntityDied(Entity<SlimeComponent> slime, ref MobStateChangedEvent args)
    {
        if (!TryComp<StorageComponent>(slime, out var storage)
            || args.NewMobState != MobState.Dead)
            return;

        var removedEnts = _container.EmptyContainer(storage.Container, true);
        foreach (var ent in removedEnts)
            _stun.TryParalyze(ent, TimeSpan.FromSeconds(5), true); // yes this is hardcoded, bite me.
    }

    private void OnEntityEscape(Entity<SlimeComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (!TryComp<PseudoItemComponent>(args.Entity, out var pseudoItem)
            || !HasComp<SlimeDamageOvertimeComponent>(args.Entity))
            return;

        RemCompDeferred<SlimeDamageOvertimeComponent>(args.Entity);

        ent.Comp.LatchedTarget = null;

        if (!pseudoItem.IntendedComp)
            RemCompDeferred(args.Entity, pseudoItem);
    }
    #endregion

    #region Helpers
    public bool NpcTryLatch(EntityUid uid, EntityUid target, SlimeComponent? component)
    {
        if (!Resolve(uid, ref component))
            return false;
        if (component.LatchedTarget.HasValue)
            return false;
        if (!HasComp<HumanoidAppearanceComponent>(target))
            return false;
        if (_mobState.IsDead(target))
            return false;
        if (!_actionBlocker.CanInteract(uid, target))
            return false;


        DoSlimeLatch(uid, target, component);
        return true;
    }

    private void DoSlimeLatch(EntityUid slime, EntityUid target, SlimeComponent slimeComp)
    {
        if (_mobState.IsDead(target))
            return;

        if (!EnsureComp<PseudoItemComponent>(target, out var pseudo))
            pseudo.IntendedComp = false;

        if (!_pseudoSystem.TryInsert(slime, target, pseudo))
        {
            var failPopup = Loc.GetString("slime-action-latch-fail", ("slime", slime), ("target", target));
            _popup.PopupEntity(failPopup, slime);

            RemCompDeferred<PseudoItemComponent>(target);
            return;
        }

        slimeComp.LatchedTarget = target;

        EnsureComp(target, out SlimeDamageOvertimeComponent comp);
        comp.SourceEntityUid = slime;

        _audio.PlayPredicted(slimeComp.EatSound, slime, slime);

        var successPopup = Loc.GetString("slime-action-latch-start", ("slime", slime), ("target", target));
        _popup.PopupEntity(successPopup, slime);

        // We also need to set a new state for the slime when it's consuming,
        // this will be easy however it's important to take MobGrowthSystem into account... possibly we should use layers?
    }
    #endregion
}
