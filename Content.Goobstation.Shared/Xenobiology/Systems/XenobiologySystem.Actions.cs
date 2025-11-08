// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Xenobiology.Components;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.DoAfter;
using Content.Shared.Humanoid;
using Content.Shared.Mobs;
using Content.Shared.Popups;
using Robust.Shared.Containers;

namespace Content.Goobstation.Shared.Xenobiology.Systems;

// This handles any actions that slime mobs may have.
public partial class XenobiologySystem
{
    private void SubscribeActions()
    {
        SubscribeLocalEvent<SlimeLatchEvent>(OnLatchAttempt);
        SubscribeLocalEvent<SlimeComponent, SlimeLatchDoAfterEvent>(OnStartLatch);

        SubscribeLocalEvent<SlimeComponent, ComponentStartup>(OnComponentInit);

        SubscribeLocalEvent<SlimeComponent, EntRemovedFromContainerMessage>(OnEntityEscape);
        SubscribeLocalEvent<SlimeComponent, MobStateChangedEvent>(OnEntityDied);

        SubscribeLocalEvent<SlimeDamageOvertimeComponent, MobStateChangedEvent>(OnConsumedEntityDied);
    }

    private void UpdateHunger()
    {
        var query = EntityQueryEnumerator<SlimeDamageOvertimeComponent>();
        while (query.MoveNext(out var uid, out var dotComp))
        {
            if (_gameTiming.CurTime < dotComp.NextTickTime
                || _mobState.IsDead(uid))
                continue;

            var addedHunger = (float)dotComp.Damage.GetTotal();
            dotComp.NextTickTime = _gameTiming.CurTime + dotComp.Interval;
            _damageable.TryChangeDamage(uid,
                dotComp.Damage,
                ignoreResistances: true,
                targetPart: TargetBodyPart.All);

            if (!_hungerQuery.TryComp(dotComp.SourceEntityUid, out var hunger)
                || dotComp.SourceEntityUid is not { } sourceEntity)
                continue;

            _hunger.ModifyHunger(sourceEntity, addedHunger, hunger);
            Dirty(sourceEntity, hunger);
        }
    }

    private void OnComponentInit(Entity<SlimeComponent> slime, ref ComponentStartup args)
    {
        slime.Comp.Stomach = _containerSystem.EnsureContainer<Container>(slime, "Stomach");
    }

    #region Events
    private void OnLatchAttempt(SlimeLatchEvent args)
    {
        if (_net.IsClient)
            return;

        var slime = args.Performer;
        var target = args.Target;

        if (TerminatingOrDeleted(target)
            || TerminatingOrDeleted(slime)
            || !_slimeQuery.TryComp(slime, out var slimeComp))
            return;

        TryDoSlimeLatch(slime, target, slimeComp);
    }

    private void OnConsumedEntityDied(Entity<SlimeDamageOvertimeComponent> ent, ref MobStateChangedEvent args)
    {
        if (_net.IsClient)
            return;

        if (_containerSystem.IsEntityOrParentInContainer(ent)
            && args.NewMobState == MobState.Dead)
            _containerSystem.TryRemoveFromContainer(ent.Owner, true);
    }

    private void OnEntityDied(Entity<SlimeComponent> slime, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead
            || _net.IsClient)
            return;

        var removedEnts = _containerSystem.EmptyContainer(slime.Comp.Stomach, true);
        foreach (var ent in removedEnts)
            _stun.TryParalyze(ent, slime.Comp.OnRemovalStunDuration, true);
    }

    private void OnEntityEscape(Entity<SlimeComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (!HasComp<SlimeDamageOvertimeComponent>(args.Entity)
            || _net.IsClient)
            return;

        RemCompDeferred<SlimeDamageOvertimeComponent>(args.Entity);
        RemCompDeferred<BeingConsumedComponent>(args.Entity);
        ent.Comp.LatchedTarget = null;
    }
    #endregion

    #region Helpers
    public bool NpcTryLatch(EntityUid uid, EntityUid target, SlimeComponent? slimeComp)
    {
        if (!Resolve(uid, ref slimeComp)
            || _net.IsClient
            || slimeComp.LatchedTarget.HasValue
            || _mobState.IsDead(target)
            || !_actionBlocker.CanInteract(uid, target)
            || !HasComp<HumanoidAppearanceComponent>(target)
            || HasComp<BeingConsumedComponent>(target))
            return false;

        TryDoSlimeLatch(uid, target, slimeComp);
        return true;
    }

    private void TryDoSlimeLatch(EntityUid slime, EntityUid target, SlimeComponent slimeComp)
    {
        if (_net.IsClient)
            return;

        if (_mobState.IsDead(target))
        {
            var targetDeadPopup = Loc.GetString("slime-latch-fail-target-dead", ("ent", target));
            _popup.PopupEntity(targetDeadPopup, slime, slime);

            return;
        }

        if (slimeComp.Stomach.Count >= slimeComp.MaxContainedEntities)
        {
            var maxEntitiesPopup = Loc.GetString("slime-latch-fail-max-entities", ("ent", target));
            _popup.PopupEntity(maxEntitiesPopup, slime, slime);

            return;
        }

        var attemptPopup = Loc.GetString("slime-latch-attempt", ("slime", slime), ("ent", target));
        _popup.PopupEntity(attemptPopup, slime, PopupType.MediumCaution);

        var doAfterArgs = new DoAfterArgs(EntityManager,
            slime,
            slimeComp.LatchDoAfterDuration,
            new SlimeLatchDoAfterEvent(),
            slime,
            target)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
        };

        EnsureComp<BeingConsumedComponent>(target);
        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnStartLatch(Entity<SlimeComponent> slime, ref SlimeLatchDoAfterEvent args)
    {
        if (args.Target is not { } target
            || _net.IsClient)
            return;

        if (args.Handled
            || args.Cancelled)
        {
            RemCompDeferred<BeingConsumedComponent>(target);
            return;
        }

        DoSlimeLatch(slime, target, slime);
        args.Handled = true;
    }

    private void DoSlimeLatch(EntityUid slime, EntityUid target, SlimeComponent slimeComp)
    {
        if (_net.IsClient)
            return;

        RemCompDeferred<BeingConsumedComponent>(target);

        if (!_containerSystem.Insert(target, slimeComp.Stomach))
        {
            var failPopup = Loc.GetString("slime-action-latch-fail", ("slime", slime), ("target", target));
            _popup.PopupEntity(failPopup, slime, PopupType.SmallCaution);

            return;
        }

        slimeComp.LatchedTarget = target;

        EnsureComp(target, out SlimeDamageOvertimeComponent comp);
        comp.SourceEntityUid = slime;

        _audio.PlayEntity(slimeComp.EatSound, slime, slime);

        var successPopup = Loc.GetString("slime-action-latch-success", ("slime", slime), ("target", target));
        _popup.PopupEntity(successPopup, slime, PopupType.SmallCaution);

        // We also need to set a new state for the slime when it's consuming,
        // this will be easy however it's important to take MobGrowthSystem into account... possibly we should use layers?
    }
    #endregion
}
