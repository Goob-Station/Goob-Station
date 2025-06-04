using Content.Goobstation.Shared.Cyberdeck;
using Content.Goobstation.Shared.Cyberdeck.Components;
using Content.Server.Emp;
using Content.Server.Light.Components;
using Content.Server.Light.EntitySystems;
using Content.Server.Power.Components;
using Content.Shared.Alert;
using Content.Shared.Body.Organ;
using Content.Shared.Charges.Systems;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;

namespace Content.Goobstation.Server.Cyberdeck;

public sealed class CyberdeckSystem : SharedCyberdeckSystem
{
    [Dependency] private readonly SharedEyeSystem _eye = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedMoverController _mover = default!;
    [Dependency] private readonly PoweredLightSystem _light = default!;
    [Dependency] private readonly EmpSystem _emp = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PoweredLightComponent, CyberdeckHackDoAfterEvent>(OnLightHacked);
        SubscribeLocalEvent<BatteryComponent, CyberdeckHackDoAfterEvent>(OnBatteryHacked);

        SubscribeLocalEvent<CyberdeckUserComponent, CyberdeckVisionEvent>(OnCyberVisionUsed);
        SubscribeLocalEvent<CyberdeckProjectionComponent, CyberdeckVisionReturnEvent>(OnCyberVisionReturn);

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

    private void OnBatteryHacked(Entity<BatteryComponent> ent, ref CyberdeckHackDoAfterEvent args)
    {
        if (!UseCharges(args.User, ent.Owner))
            return;

        var mapPos = _transform.GetMapCoordinates(ent.Owner);
        var radius = ent.Comp.MaxCharge / ent.Comp.CurrentCharge * 2.5f;
        var duration = ent.Comp.MaxCharge / ent.Comp.CurrentCharge * 10;

        _emp.EmpPulse(mapPos, radius, ent.Comp.MaxCharge, duration);
    }

    private void OnLightHacked(Entity<PoweredLightComponent> ent, ref CyberdeckHackDoAfterEvent args)
    {
        if (!UseCharges(args.User, ent.Owner))
            return;

        _light.TryDestroyBulb(ent.Owner, ent.Comp);
    }

    private void OnCyberVisionUsed(Entity<CyberdeckUserComponent> ent, ref CyberdeckVisionEvent args)
    {
        if (args.Handled)
            return;

        var (uid, comp) = ent;

        UseCharges(uid, comp.CyberVisionAbilityCost);
        SetupProjection(ent);
        Actions.AddAction(uid, ref comp.ReturnAction, comp.ReturnActionId);
        Actions.RemoveAction(uid, comp.VisionAction);
        args.Handled = true;
    }

    private void OnCyberVisionReturn(Entity<CyberdeckProjectionComponent> ent, ref CyberdeckVisionReturnEvent args)
    {
        if (args.Handled)
            return;

        ShutdownProjection(ent.Owner);
        args.Handled = true;
    }

    private void SetupProjection(Entity<CyberdeckUserComponent> user)
    {
        // Shutdown an already existing projection, if it really exists.
        ShutdownProjection(user.Comp.ProjectionEntity);

        var position = Transform(user.Owner).Coordinates;
        var observer = Spawn(user.Comp.ProjectionEntityId, position);
        _transform.AttachToGridOrMap(observer);

        EnsureComp<CyberdeckProjectionComponent>(observer).RemoteEntity = user.Owner;

        if (TryComp(user, out EyeComponent? eyeComp))
        {
            _eye.SetDrawFov(user, false, eyeComp);
            _eye.SetTarget(user, observer, eyeComp);
        }

        _mover.SetRelay(user, observer);
        user.Comp.ProjectionEntity = observer;
    }

    protected override void ShutdownProjection(Entity<CyberdeckProjectionComponent?>? ent)
    {
        if (ent == null)
            return;

        var comp = ent.Value.Comp;

        if (!Resolve(ent.Value.Owner, ref comp)
            || !UserQuery.TryComp(comp.RemoteEntity, out var userComp))
            return;

        var user = comp.RemoteEntity.Value;
        userComp.ProjectionEntity = null;
        Actions.RemoveAction(user, userComp.ReturnAction);
        Actions.AddAction(user, ref userComp.VisionAction, userComp.VisionActionId);
        Dirty(ent.Value.Owner, comp);

        if (TryComp(user, out EyeComponent? eyeComp))
        {
            _eye.SetDrawFov(user, true, eyeComp);
            _eye.SetTarget(user, user, eyeComp);
        }

        RemComp<RelayInputMoverComponent>(user);
        QueueDel(ent.Value.Owner);
    }
}
