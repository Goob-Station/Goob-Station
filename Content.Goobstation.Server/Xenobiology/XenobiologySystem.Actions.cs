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
using Content.Shared.Examine;
using Content.Shared.Humanoid;
using Content.Shared.Mobs;
using Content.Shared.Nutrition.Components;
using Content.Shared.Popups;
using Robust.Shared.Containers;

namespace Content.Goobstation.Server.Xenobiology;

/// <summary>
/// This handles any actions that slime mobs may have.
/// </summary>
public partial class XenobiologySystem
{
    private void InitializeActions()
    {
        SubscribeLocalEvent<SlimeLatchEvent>(OnLatch);

        SubscribeLocalEvent<SlimeComponent, ComponentStartup>(OnComponentInit);
        SubscribeLocalEvent<SlimeComponent, ExaminedEvent>(OnExamined);

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

            if (!TryComp<HungerComponent>(dotComp.SourceEntityUid, out var hunger)
                || dotComp.SourceEntityUid is not { } sourceEntity)
                continue;

            _hunger.ModifyHunger(sourceEntity, addedHunger, hunger);
            Dirty(sourceEntity, hunger);
        }
    }

    private void OnComponentInit(Entity<SlimeComponent> slime, ref ComponentStartup args) =>
        slime.Comp.Stomach = _containerSystem.EnsureContainer<Container>(slime, "Stomach");

    private void OnExamined(Entity<SlimeComponent> slime, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange
            || slime.Comp.Stomach.Count <= 0)
            return;

        var text = Loc.GetString("slime-examined-text", ("num", slime.Comp.Stomach.Count));
        args.PushMarkup(text);
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
        if (_containerSystem.IsEntityOrParentInContainer(ent)
            && args.NewMobState == MobState.Dead)
            _containerSystem.TryRemoveFromContainer(ent, true);
    }

    private void OnEntityDied(Entity<SlimeComponent> slime, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        var removedEnts = _containerSystem.EmptyContainer(slime.Comp.Stomach, true);
        foreach (var ent in removedEnts)
            _stun.TryParalyze(ent, slime.Comp.OnRemovalStunDuration, true);
    }

    private void OnEntityEscape(Entity<SlimeComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (!HasComp<SlimeDamageOvertimeComponent>(args.Entity))
            return;

        RemCompDeferred<SlimeDamageOvertimeComponent>(args.Entity);
        ent.Comp.LatchedTarget = null;
    }
    #endregion

    #region Helpers
    public bool NpcTryLatch(EntityUid uid, EntityUid target, SlimeComponent? slimeComp)
    {
        if (!Resolve(uid, ref slimeComp)
            || slimeComp.LatchedTarget.HasValue
            || !HasComp<HumanoidAppearanceComponent>(target)
            || _mobState.IsDead(target)
            || !_actionBlocker.CanInteract(uid, target))
            return false;

        DoSlimeLatch(uid, target, slimeComp);
        return true;
    }

    private void DoSlimeLatch(EntityUid slime, EntityUid target, SlimeComponent slimeComp)
    {
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
