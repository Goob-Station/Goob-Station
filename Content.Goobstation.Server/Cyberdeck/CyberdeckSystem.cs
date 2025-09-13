// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Cyberdeck.Components;
using Content.Goobstation.Shared.Cyberdeck;
using Content.Server.Atmos.Monitor.Components;
using Content.Server.Atmos.Monitor.Systems;
using Content.Server.Chat.Managers;
using Content.Server.Emp;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Light.Components;
using Content.Server.Light.EntitySystems;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Server.VendingMachines;
using Content.Shared.Atmos.Monitor.Components;
using Content.Shared.Charges.Components;
using Content.Shared.Chat;
using Content.Shared.DeviceNetwork.Components;
using Content.Shared.Explosion.Components;
using Content.Shared.IdentityManagement;
using Content.Shared.Popups;
using Content.Shared.VendingMachines;
using Robust.Shared.Physics.Components;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.Cyberdeck;

public sealed class CyberdeckSystem : SharedCyberdeckSystem
{
    // Imagine a world where all of these systems are predicted...
    [Dependency] private readonly AirAlarmSystem _airAlarm = default!;
    [Dependency] private readonly ApcSystem _apcSystem = default!;
    [Dependency] private readonly BatterySystem _battery = default!;
    [Dependency] private readonly EmpSystem _emp = default!;
    [Dependency] private readonly ExplosionSystem _explosion = default!;
    [Dependency] private readonly PoweredLightSystem _light = default!;
    [Dependency] private readonly VendingMachineSystem _vending = default!;
    [Dependency] private readonly IChatManager _chat = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AirAlarmComponent, CyberdeckHackDeviceEvent>(OnAirAlarmHacked);
        SubscribeLocalEvent<ApcComponent, CyberdeckHackDeviceEvent>(OnApcHacked);
        SubscribeLocalEvent<BatteryComponent, CyberdeckHackDeviceEvent>(OnBatteryHacked);
        SubscribeLocalEvent<PoweredLightComponent, CyberdeckHackDeviceEvent>(OnLightHacked);
        SubscribeLocalEvent<PowerNetworkBatteryComponent, CyberdeckHackDeviceEvent>(OnPowerNetworkHacked);
        SubscribeLocalEvent<VendingMachineComponent, CyberdeckHackDeviceEvent>(OnVendingHacked);
        SubscribeLocalEvent<CyberdeckUserComponent, CyberdeckInfoAlertEvent>(OnUserAlertClicked);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // Because AutoRechargeComponent doesn't have any loops, we have to update everything by ourselves.
        // Another total optimization L.
        var query = EntityQueryEnumerator<CyberdeckSourceComponent, AutoRechargeComponent, LimitedChargesComponent>();
        while (query.MoveNext(out var uid, out var sourceComp, out var rechargeComp, out var chargesComp))
        {
            // To stay in sync, we need to start updating this after the first charge is used.
            if (sourceComp.Accumulator == null)
                continue;

            sourceComp.Accumulator -= frameTime;

            if (sourceComp.Accumulator > 0)
                continue;

            // Update charges amount by removing and adding back one charge.
            Charges.AddCharges((uid, chargesComp, rechargeComp), -1);
            Charges.AddCharges((uid, chargesComp, rechargeComp), 1);

            sourceComp.Accumulator = (float) rechargeComp.RechargeDuration.TotalSeconds;
        }
    }

    private void OnBatteryHacked(Entity<BatteryComponent> ent, ref CyberdeckHackDeviceEvent args)
    {
        var mass = 60.0f; // This is probably something wall-mount if it doesn't have any physics
        if (TryComp(ent.Owner, out PhysicsComponent? physics))
            mass = physics.FixturesMass;

        // Safety measure from YAMLmaxxers
        mass = Math.Min(mass, 1000f);

        var mapPos = Xform.GetMapCoordinates(ent.Owner);
        var percentage = ent.Comp.CurrentCharge / Math.Max(ent.Comp.MaxCharge, 1f);

        // A power-cell is 5 kg and SMES is ~150, so at 100% charge
        // a powercell will hit ~1.1 tile radius, SMES ~6.1, and an extreme case is ~16 tiles.
        var radius = percentage * MathF.Sqrt(mass) / 2;
        var duration = percentage * 10; // 0-10 seconds

        // Less than 5% does nothing, just silently drains all remaining battery
        if (percentage < 0.05f)
        {
            _battery.SetCharge(ent.Owner, 0f, ent.Comp);
            return;
        }

        // bazillions IPC must die
        _emp.EmpPulse(mapPos, radius, ent.Comp.CurrentCharge, duration);

        // Validhunt must spread
        var message = Loc.GetString("cyberdeck-battery-get-hacked",
            ("target", Identity.Entity(ent.Owner, EntityManager, args.User)));

        Popup.PopupEntity(message, ent.Owner, PopupType.Large);
    }

    private void OnAirAlarmHacked(Entity<AirAlarmComponent> ent, ref CyberdeckHackDeviceEvent args)
    {
        var addr = string.Empty;
        if (TryComp<DeviceNetworkComponent>(ent.Owner, out var netConn))
            addr = netConn.Address;

        _airAlarm.SetMode(ent.Owner, addr, AirAlarmMode.Panic, false, ent.Comp);
    }

    private void OnApcHacked(Entity<ApcComponent> ent, ref CyberdeckHackDeviceEvent args)
        => _apcSystem.ApcToggleBreaker(ent.Owner, ent.Comp);

    private void OnLightHacked(Entity<PoweredLightComponent> ent, ref CyberdeckHackDeviceEvent args)
        => args.Refund = !_light.TryDestroyBulb(ent.Owner, ent.Comp);

    private void OnPowerNetworkHacked(Entity<PowerNetworkBatteryComponent> ent, ref CyberdeckHackDeviceEvent args)
    {
        if (TryComp(ent.Owner, out ExplosiveComponent? explosive))
            _explosion.TriggerExplosive(ent.Owner, explosive, user: args.User);
    }

    private void OnVendingHacked(Entity<VendingMachineComponent> ent, ref CyberdeckHackDeviceEvent args)
        => _vending.EjectRandom(ent.Owner, true, true, ent.Comp);

    private void OnUserAlertClicked(Entity<CyberdeckUserComponent> ent, ref CyberdeckInfoAlertEvent args)
    {
        if (args.Handled
            || !TryComp<ActorComponent>(ent, out var actor)
            || !TryComp<LimitedChargesComponent>(ent.Comp.ProviderEntity, out var chargesComp)
            || !TryComp<AutoRechargeComponent>(ent.Comp.ProviderEntity, out var rechargeComp))
            return;

        var session = actor.PlayerSession;
        var chargesAmount = Charges.GetCurrentCharges((ent.Comp.ProviderEntity.Value, chargesComp, rechargeComp));
        var rechargeTime = (int) rechargeComp.RechargeDuration.TotalSeconds;
        var msgStart =
            Loc.GetString("cyberdeck-get-alert-info",
                ("chargesAmount", chargesAmount),
                ("rechargeTime", rechargeTime));

        _chat.ChatMessageToOne(ChatChannel.Emotes,
            msgStart,
            msgStart,
            EntityUid.Invalid,
            false,
            session.Channel);

        args.Handled = true;
    }
}
