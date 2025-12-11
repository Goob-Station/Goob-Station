// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Xenobiology;
using Content.Goobstation.Shared.Xenobiology.Components;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.ActionBlocker;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Humanoid;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Xenobiology.Systems;

// This handles any actions that slime mobs may have.
public sealed partial class SlimeLatchSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly HungerSystem _hunger = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlimeLatchEvent>(OnLatchAttempt);
        SubscribeLocalEvent<SlimeComponent, SlimeLatchDoAfterEvent>(OnStartLatch);

        SubscribeLocalEvent<SlimeComponent, EntRemovedFromContainerMessage>(OnEntityEscape);
        SubscribeLocalEvent<SlimeComponent, MobStateChangedEvent>(OnEntityDied);

        SubscribeLocalEvent<SlimeDamageOvertimeComponent, MobStateChangedEvent>(OnConsumedEntityDied);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<SlimeDamageOvertimeComponent>();
        while (query.MoveNext(out var uid, out var dotComp))
        {
            if (_gameTiming.CurTime < dotComp.NextTickTime || _mobState.IsDead(uid))
                continue;

            var addedHunger = (float) dotComp.Damage.GetTotal();
            dotComp.NextTickTime = _gameTiming.CurTime + dotComp.Interval;
            _damageable.TryChangeDamage(uid, dotComp.Damage, ignoreResistances: true, targetPart: TargetBodyPart.All);

            if (!TryComp<HungerComponent>(dotComp.SourceEntityUid, out var hunger) || dotComp.SourceEntityUid is not { } sourceEntity)
                continue;

            _hunger.ModifyHunger(sourceEntity, addedHunger, hunger);
            Dirty(sourceEntity, hunger);
        }
    }

    private void OnLatchAttempt(SlimeLatchEvent args)
    {
        if (TerminatingOrDeleted(args.Target)
        || TerminatingOrDeleted(args.Performer)
        || !TryComp<SlimeComponent>(args.Performer, out var slime))
            return;

        TryDoSlimeLatch((args.Performer, slime), args.Target);
    }

    private void OnConsumedEntityDied(Entity<SlimeDamageOvertimeComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        var source = ent.Comp.SourceEntityUid;
        if (source.HasValue && TryComp<SlimeComponent>(source, out var slime))
            Unlatch((source.Value, slime));
    }

    private void OnEntityDied(Entity<SlimeComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        Unlatch(ent);
    }

    private void OnEntityEscape(Entity<SlimeComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (!HasComp<SlimeDamageOvertimeComponent>(args.Entity))
            return;

        RemCompDeferred<SlimeDamageOvertimeComponent>(args.Entity);
        RemCompDeferred<BeingConsumedComponent>(args.Entity);
        ent.Comp.LatchedTarget = null;
    }

    #region Helpers

    public bool IsLatched(Entity<SlimeComponent> ent, EntityUid target)
    {
        return ent.Comp.LatchedTarget.HasValue && ent.Comp.LatchedTarget.Value == target;
    }

    public bool NpcTryLatch(Entity<SlimeComponent> ent, EntityUid target)
    {
        if (ent.Comp.LatchedTarget.HasValue
            || _mobState.IsDead(target)
            || !_actionBlocker.CanInteract(ent, target)
            || !HasComp<HumanoidAppearanceComponent>(target)
            || HasComp<BeingConsumedComponent>(target))
            return false;

        TryDoSlimeLatch(ent, target);
        return true;
    }

    private void TryDoSlimeLatch(Entity<SlimeComponent> ent, EntityUid target)
    {
        if (_mobState.IsDead(target))
        {
            var targetDeadPopup = Loc.GetString("slime-latch-fail-target-dead", ("ent", target));
            _popup.PopupEntity(targetDeadPopup, ent, ent);

            return;
        }

        if (ent.Comp.Stomach.Count >= ent.Comp.MaxContainedEntities)
        {
            var maxEntitiesPopup = Loc.GetString("slime-latch-fail-max-entities", ("ent", target));
            _popup.PopupEntity(maxEntitiesPopup, ent, ent);

            return;
        }

        var attemptPopup = Loc.GetString("slime-latch-attempt", ("slime", ent), ("ent", target));
        _popup.PopupEntity(attemptPopup, ent, PopupType.MediumCaution);

        var doAfterArgs = new DoAfterArgs(EntityManager, ent, ent.Comp.LatchDoAfterDuration, new SlimeLatchDoAfterEvent(), ent, target)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
        };

        EnsureComp<BeingConsumedComponent>(target);
        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnStartLatch(Entity<SlimeComponent> ent, ref SlimeLatchDoAfterEvent args)
    {
        if (args.Target is not { } target)
            return;

        if (args.Handled || args.Cancelled)
        {
            RemCompDeferred<BeingConsumedComponent>(target);
            return;
        }

        Latch(ent, target);
        args.Handled = true;
    }

    private void Latch(Entity<SlimeComponent> ent, EntityUid target)
    {
        RemCompDeferred<BeingConsumedComponent>(target);

        _xform.SetParent(ent, target);

        ent.Comp.LatchedTarget = target;

        EnsureComp(target, out SlimeDamageOvertimeComponent comp);
        comp.SourceEntityUid = ent;

        _audio.PlayEntity(ent.Comp.EatSound, ent, ent);

        var successPopup = Loc.GetString("slime-action-latch-success", ("slime", ent), ("target", target));
        _popup.PopupEntity(successPopup, ent, PopupType.SmallCaution);

        // We also need to set a new state for the slime when it's consuming,
        // this will be easy however it's important to take MobGrowthSystem into account... possibly we should use layers?
    }

    private void Unlatch(Entity<SlimeComponent> ent)
    {
        if (!ent.Comp.LatchedTarget.HasValue)
            return;

        var target = ent.Comp.LatchedTarget.Value;

        RemCompDeferred<BeingConsumedComponent>(target);
        RemCompDeferred<SlimeDamageOvertimeComponent>(target);
        _xform.SetParent(ent, _xform.GetParentUid(target)); // deparent it. probably.
    }

    #endregion
}
