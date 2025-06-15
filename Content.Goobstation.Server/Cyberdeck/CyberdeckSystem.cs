// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Cyberdeck.Components;
using Content.Goobstation.Shared.Cyberdeck;
using Content.Server.Emp;
using Content.Server.Light.Components;
using Content.Server.Light.EntitySystems;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.Alert;
using Content.Shared.Body.Organ;
using Content.Shared.Charges.Systems;
using Content.Shared.IdentityManagement;
using Content.Shared.Popups;

namespace Content.Goobstation.Server.Cyberdeck;


public sealed class CyberdeckSystem : SharedCyberdeckSystem
{
    // Imagine a world where all of these systems are predicted...
    [Dependency] private readonly PoweredLightSystem _light = default!;
    [Dependency] private readonly EmpSystem _emp = default!;
    [Dependency] private readonly BatterySystem _battery = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PoweredLightComponent, CyberdeckHackDeviceEvent>(OnLightHacked);
        SubscribeLocalEvent<BatteryComponent, CyberdeckHackDeviceEvent>(OnBatteryHacked);

        SubscribeLocalEvent<CyberdeckSourceComponent, ChargesChangedEvent>(OnChargesChanged);
    }

    private void OnChargesChanged(Entity<CyberdeckSourceComponent> ent, ref ChargesChangedEvent args)
    {
        if (!TryComp(ent.Owner, out OrganComponent? organ)
            || !UserQuery.TryComp(organ.Body, out var userComp))
            return;

        var user = organ.Body.Value;
        var charges = (short) Math.Clamp(args.CurrentCharges.Int(), 0, 8);
        _alerts.ShowAlert(user, userComp.AlertId, charges);
    }

    private void OnBatteryHacked(Entity<BatteryComponent> ent, ref CyberdeckHackDeviceEvent args)
    {
        // TODO: this mostly works just with items, and can't process high-power structures properly.
        // Im bad at math so if you're going to make something like Substations cyberdeck-hackable, please change this code first.

        var mapPos = Xform.GetMapCoordinates(ent.Owner);
        var percentage = ent.Comp.CurrentCharge / ent.Comp.MaxCharge;
        var radius = percentage * 2.5f;
        var duration = percentage * 10;

        if (percentage < 0.1f)
        {
            // Less than 10% does nothing, just silently drains all remaining battery
            _battery.SetCharge(ent.Owner, 0f, ent.Comp);
            return;
        }

        // bazillions IPCs must die
        _emp.EmpPulse(mapPos, radius, ent.Comp.CurrentCharge, duration);

        // Validhunt must spread
        var message = Loc.GetString("cyberdeck-battery-get-hacked",
            ("target", Identity.Entity(ent.Owner, EntityManager, args.User)));

        Popup.PopupEntity(message, ent.Owner, PopupType.Large);
    }

    private void OnLightHacked(Entity<PoweredLightComponent> ent, ref CyberdeckHackDeviceEvent args)
    {
        args.Refund = !_light.TryDestroyBulb(ent.Owner, ent.Comp);
    }

    /// <inheritdoc/>
    protected override void UpdateAlert(Entity<CyberdeckUserComponent> ent, bool doClear = false)
    {
        if (doClear)
        {
            _alerts.ClearAlert(ent.Owner, ent.Comp.AlertId);
            return;
        }

        if (!ChargesQuery.TryComp(ent.Comp.ProviderEntity, out var chargesComp))
            return;

        var charges = chargesComp.LastCharges;
        var severity = (short) Math.Clamp(charges, 0, 8);
        _alerts.ShowAlert(ent.Owner, ent.Comp.AlertId, severity);
    }
}
