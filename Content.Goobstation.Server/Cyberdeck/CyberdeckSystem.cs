// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Cyberdeck.Components;
using Content.Goobstation.Common.Interaction;
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
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Silicons.StationAi;

namespace Content.Goobstation.Server.Cyberdeck;

public sealed class CyberdeckSystem : SharedCyberdeckSystem
{
    [Dependency] private readonly SharedEyeSystem _eye = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedMoverController _mover = default!;
    [Dependency] private readonly PoweredLightSystem _light = default!;
    [Dependency] private readonly EmpSystem _emp = default!;
    [Dependency] private readonly BatterySystem _battery = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PoweredLightComponent, CyberdeckHackDeviceEvent>(OnLightHacked);
        SubscribeLocalEvent<BatteryComponent, CyberdeckHackDeviceEvent>(OnBatteryHacked);

        SubscribeLocalEvent<CyberdeckUserComponent, CyberdeckVisionEvent>(OnCyberVisionUsed);
        SubscribeLocalEvent<CyberdeckUserComponent, CyberdeckVisionReturnEvent>(OnCyberVisionReturn);

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

        var mapPos = _transform.GetMapCoordinates(ent.Owner);
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
        // Seriously. They don't predict light bulbs????????
        args.Refund = !_light.TryDestroyBulb(ent.Owner, ent.Comp);
    }

    private void OnCyberVisionUsed(Entity<CyberdeckUserComponent> ent, ref CyberdeckVisionEvent args)
    {
        if (args.Handled)
            return;

        var (uid, comp) = ent;

        if (!UseCharges(uid, comp.CyberVisionAbilityCost))
            return;

        SetupProjection(ent);
        args.Handled = true;
    }

    private void OnCyberVisionReturn(Entity<CyberdeckUserComponent> ent, ref CyberdeckVisionReturnEvent args)
    {
        if (args.Handled)
            return;

        ShutdownProjection(ent.Comp.ProjectionEntity);
        args.Handled = true;
    }

    private void SetupProjection(Entity<CyberdeckUserComponent> user)
    {
        // Shutdown an already existing projection, if it really exists.
        ShutdownProjection(user.Comp.ProjectionEntity);

        var position = Transform(user.Owner).Coordinates;
        var observer = SpawnAtPosition(user.Comp.ProjectionEntityId, position);

        EnsureComp<CyberdeckProjectionComponent>(observer).RemoteEntity = user.Owner;

        if (TryComp(user, out EyeComponent? eyeComp))
        {
            _eye.SetDrawFov(user, false, eyeComp);
            _eye.SetTarget(user, observer, eyeComp);
        }

        _mover.SetRelay(user, observer);

        EnsureComp<StationAiOverlayComponent>(user.Owner);
        EnsureComp<CyberdeckOverlayComponent>(user.Owner);
        EnsureComp<NoNormalInteractionComponent>(user.Owner);

        Actions.AddAction(user.Owner, ref user.Comp.ReturnAction, user.Comp.ReturnActionId);
        Actions.RemoveAction(user.Owner, user.Comp.VisionAction);

        user.Comp.ProjectionEntity = observer;
        DirtyEntity(user.Owner);
    }

    protected override void ShutdownProjection(Entity<CyberdeckProjectionComponent?>? ent)
    {
        if (ent == null || TerminatingOrDeleted(ent.Value.Owner))
            return;

        var comp = ent.Value.Comp;

        if (!Resolve(ent.Value.Owner, ref comp)
            || !UserQuery.TryComp(comp.RemoteEntity, out var userComp))
            return;

        var user = comp.RemoteEntity.Value;

        RemComp<StationAiOverlayComponent>(user);
        RemComp<CyberdeckOverlayComponent>(user);
        RemComp<NoNormalInteractionComponent>(user);

        Actions.AddAction(user, ref userComp.VisionAction, userComp.VisionActionId);
        Actions.RemoveAction(user, userComp.ReturnAction);

        if (TryComp(user, out EyeComponent? eyeComp))
            _eye.SetDrawFov(user, true, eyeComp);

        DirtyEntity(user);
        QueueDel(ent.Value.Owner);
    }

    protected override void UpdateAlert(Entity<CyberdeckUserComponent> ent)
    {
        if (!ChargesQuery.TryComp(ent.Comp.ProviderEntity, out var chargesComp))
            return;

        var charges = chargesComp.Charges;
        var severity = (short) Math.Clamp(charges.Int(), 0, 8);
        _alerts.ShowAlert(ent.Owner, ent.Comp.AlertId, severity);
    }
}
