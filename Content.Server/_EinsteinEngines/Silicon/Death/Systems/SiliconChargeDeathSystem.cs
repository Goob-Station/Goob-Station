// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Power.Components;
using Content.Shared._EinsteinEngines.Silicon.Systems;
using Content.Shared.Bed.Sleep;
using Content.Server._EinsteinEngines.Silicon.Charge;
using Content.Server.Humanoid;
using Content.Shared.Humanoid;
using Content.Shared.StatusEffectNew;
using Content.Shared.Hands.Components; // Monolith - IPC rework.
using Content.Shared.Hands.EntitySystems;
// Goobstation Start - Energycrit
using Content.Goobstation.Shared.Sprinting;
using Content.Server.Radio;
using Content.Shared.Actions;
using Content.Shared.CombatMode;
using Content.Shared.Standing;
using Content.Shared.Stunnable;
// Goobstation End - Energycrit

namespace Content.Server._EinsteinEngines.Silicon.Death;

public sealed class SiliconDeathSystem : EntitySystem
{
    [Dependency] private readonly SleepingSystem _sleep = default!;
    [Dependency] private readonly SharedStatusEffectsSystem _status = default!;
    [Dependency] private readonly SiliconChargeSystem _silicon = default!;
    [Dependency] private readonly HumanoidAppearanceSystem _humanoidAppearanceSystem = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!; // Monolith - IPC rework
    // Goobstation Start - Energycrit
    [Dependency] private readonly SharedCombatModeSystem _combat = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;
    // Goobstation End - Energycrit

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SiliconDownOnDeadComponent, SiliconChargeStateUpdateEvent>(OnSiliconChargeStateUpdate);

        // Goobstation Start - Energycrit
        SubscribeLocalEvent<SiliconDownOnDeadComponent, RadioSendAttemptEvent>(OnRadioSendAttempt);
        SubscribeLocalEvent<SiliconDownOnDeadComponent, StandAttemptEvent>(OnStandAttempt);
        // Goobstation End - Energycrit
    }

    private void OnSiliconChargeStateUpdate(EntityUid uid, SiliconDownOnDeadComponent siliconDeadComp, SiliconChargeStateUpdateEvent args)
    {
        if (!_silicon.TryGetSiliconBattery(uid, out var batteryComp))
        {
            SiliconDead(uid, siliconDeadComp, batteryComp, uid);
            return;
        }

        if (args.ChargePercent == 0 && siliconDeadComp.Dead)
            return;

        if (args.ChargePercent == 0 && !siliconDeadComp.Dead)
            SiliconDead(uid, siliconDeadComp, batteryComp, uid);
        else if (args.ChargePercent != 0 && siliconDeadComp.Dead)
            SiliconUnDead(uid, siliconDeadComp, batteryComp, uid);
    }

    /// <Goobstation> Energycrit </Goobstation>
    private void OnRadioSendAttempt(Entity<SiliconDownOnDeadComponent> ent, ref RadioSendAttemptEvent args)
    {
        // Prevent talking on radio if energycrit
        if (args.Cancelled || !ent.Comp.Dead)
            return;

        args.Cancelled = true;
    }

    /// <Goobstation> Energycrit </Goobstation>
    /// <summary>
    ///     Some actions, like picking up an IPC and carrying it remove the KnockedDownComponent, if they try to stand when they
    ///     shouldn't, just knock them down again
    /// </summary>
    private void OnStandAttempt(Entity<SiliconDownOnDeadComponent> ent, ref StandAttemptEvent args)
    {
        // Prevent standing up if energycrit
        if (args.Cancelled || !ent.Comp.Dead)
            return;

        EnsureComp<KnockedDownComponent>(ent);
        args.Cancel();
    }

    private void SiliconDead(EntityUid uid, SiliconDownOnDeadComponent siliconDeadComp, BatteryComponent? batteryComp, EntityUid batteryUid)
    {
        var deadEvent = new SiliconChargeDyingEvent(uid, batteryComp, batteryUid);
        RaiseLocalEvent(uid, deadEvent);

        if (deadEvent.Cancelled)
            return;

        // Monolith Start - IPC rework
        /*
        EntityManager.EnsureComponent<SleepingComponent>(uid);
        // Im too lazy to rewrite fucking stupid API so instead of sleeping infinitely IPCs will sleep for 2 damn days.
        _status.TryAddStatusEffectDuration(uid, "StatusEffectForcedSleeping", TimeSpan.FromDays(2));
        */
        if (!TryComp<HandsComponent>(uid, out var handsComp))
            return;
        _hands.RemoveHands((uid, handsComp));
        // Monolith End - IPC rework

        // Goobstation Start - Energycrit
        // Disable sprinting.
        if (TryComp<SprinterComponent>(uid, out var sprint))
        {
            sprint.CanSprint = false;
            Dirty(uid, sprint);
        }

        // Disable combat mode
        if (TryComp<CombatModeComponent>(uid, out var combatMode))
        {
            _combat.SetInCombatMode(uid, false);
            _actions.SetEnabled(combatMode.CombatToggleActionEntity, false);
        }

        // Knock down
        _standing.Down(uid);
        EnsureComp<KnockedDownComponent>(uid);

        // Goobstation End - Energycrit

        if (TryComp(uid, out HumanoidAppearanceComponent? humanoidAppearanceComponent))
        {
            var layers = HumanoidVisualLayersExtension.Sublayers(HumanoidVisualLayers.HeadSide);
            _humanoidAppearanceSystem.SetLayersVisibility((uid, humanoidAppearanceComponent), layers, false);
        }

        siliconDeadComp.Dead = true;

        RaiseLocalEvent(uid, new SiliconChargeDeathEvent(uid, batteryComp, batteryUid));
    }

    private void SiliconUnDead(EntityUid uid, SiliconDownOnDeadComponent siliconDeadComp, BatteryComponent? batteryComp, EntityUid batteryUid)
    {
        // Monolith Start - IPC Rework
        /*
        _status.TryRemoveStatusEffect(uid, "StatusEffectForcedSleeping");
        _sleep.TryWaking(uid, true);
        */
        _hands.AddHand(uid, "right hand", HandLocation.Right);
        _hands.AddHand(uid, "left hand", HandLocation.Left);
        // Monolith End - IPC rework

        // Goobstation Start - Energycrit
        // Enable sprinting
        if (TryComp<SprinterComponent>(uid, out var sprint))
        {
            sprint.CanSprint = true;
            Dirty(uid, sprint);
        }

        // Enable combat mode
        if (TryComp<CombatModeComponent>(uid, out var combatMode))
            _actions.SetEnabled(combatMode.CombatToggleActionEntity, true);

        // Let you stand again
        RemComp<KnockedDownComponent>(uid);

        // Goobstation End - Energycrit

        siliconDeadComp.Dead = false;

        RaiseLocalEvent(uid, new SiliconChargeAliveEvent(uid, batteryComp, batteryUid));
    }
}

/// <summary>
///     A cancellable event raised when a Silicon is about to go down due to charge.
/// </summary>
/// <remarks>
///     This probably shouldn't be modified unless you intend to fill the Silicon's battery,
///     as otherwise it'll just be triggered again next frame.
/// </remarks>
public sealed class SiliconChargeDyingEvent : CancellableEntityEventArgs
{
    public EntityUid SiliconUid { get; }
    public BatteryComponent? BatteryComp { get; }
    public EntityUid BatteryUid { get; }

    public SiliconChargeDyingEvent(EntityUid siliconUid, BatteryComponent? batteryComp, EntityUid batteryUid)
    {
        SiliconUid = siliconUid;
        BatteryComp = batteryComp;
        BatteryUid = batteryUid;
    }
}

/// <summary>
///     An event raised after a Silicon has gone down due to charge.
/// </summary>
public sealed class SiliconChargeDeathEvent : EntityEventArgs
{
    public EntityUid SiliconUid { get; }
    public BatteryComponent? BatteryComp { get; }
    public EntityUid BatteryUid { get; }

    public SiliconChargeDeathEvent(EntityUid siliconUid, BatteryComponent? batteryComp, EntityUid batteryUid)
    {
        SiliconUid = siliconUid;
        BatteryComp = batteryComp;
        BatteryUid = batteryUid;
    }
}

/// <summary>
///     An event raised after a Silicon has reawoken due to an increase in charge.
/// </summary>
public sealed class SiliconChargeAliveEvent : EntityEventArgs
{
    public EntityUid SiliconUid { get; }
    public BatteryComponent? BatteryComp { get; }
    public EntityUid BatteryUid { get; }

    public SiliconChargeAliveEvent(EntityUid siliconUid, BatteryComponent? batteryComp, EntityUid batteryUid)
    {
        SiliconUid = siliconUid;
        BatteryComp = batteryComp;
        BatteryUid = batteryUid;
    }
}
