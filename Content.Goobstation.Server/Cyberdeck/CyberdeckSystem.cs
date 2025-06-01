using Content.Goobstation.Shared.Cyberdeck;
using Content.Goobstation.Shared.Cyberdeck.Components;
using Content.Server.Emp;
using Content.Server.Light.Components;
using Content.Server.Light.EntitySystems;
using Content.Server.Power.Components;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;

namespace Content.Goobstation.Server.Cyberdeck;

public sealed class CyberdeckSystem : SharedCyberdeckSystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly PoweredLightSystem _light = default!;
    [Dependency] private readonly EmpSystem _emp = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PoweredLightComponent, CyberdeckHackDoAfterEvent>(OnLightHacked);
        SubscribeLocalEvent<BatteryComponent, CyberdeckHackDoAfterEvent>(OnBatteryHacked);

        SubscribeLocalEvent<CyberdeckUserComponent, CyberdeckVisionEvent>(OnCyberVisionUsed);
        SubscribeLocalEvent<CyberdeckProjectionComponent, CyberdeckVisionReturnEvent>(OnCyberVisionReturn);
    }

    protected override void ShutdownProjection(Entity<CyberdeckProjectionComponent> ent)
    {
        if (!TryComp<VisitingMindComponent>(ent.Owner, out var mindId)
            || mindId.MindId == null
            || !TryComp<MindComponent>(mindId.MindId.Value, out var mind))
            return;

        _mind.UnVisit(mindId.MindId.Value, mind);
        QueueDel(ent.Owner);
    }

    private void OnBatteryHacked(Entity<BatteryComponent> ent, ref CyberdeckHackDoAfterEvent args)
    {
        if (!UseCharges(ent.Owner, args.User))
            return;

        var mapPos = _transform.GetMapCoordinates(ent.Owner);
        var radius = ent.Comp.MaxCharge / ent.Comp.CurrentCharge * 2.5f;
        var duration = ent.Comp.MaxCharge / ent.Comp.CurrentCharge * 10;

        _emp.EmpPulse(mapPos, radius, ent.Comp.MaxCharge, duration);
    }

    private void OnLightHacked(Entity<PoweredLightComponent> ent, ref CyberdeckHackDoAfterEvent args)
    {
        if (!UseCharges(ent.Owner, args.User))
            return;

        _light.TryDestroyBulb(ent.Owner, ent.Comp);
    }

    private void OnCyberVisionUsed(Entity<CyberdeckUserComponent> ent, ref CyberdeckVisionEvent args)
    {
        var (uid, comp) = ent;

        if (args.Handled
            || comp.ProviderEntity != null
            && Charges.IsEmpty(comp.ProviderEntity.Value))
            return;

        if (comp.ProviderEntity != null)
            Charges.UseCharges(comp.ProviderEntity.Value, comp.CyberVisionAbilityCost);

        if (!_mind.TryGetMind(uid, out var mind, out var mindComp))
            return;

        var position = Transform(uid).Coordinates;
        var observer = Spawn(comp.ProjectionEntityId, position);
        _transform.AttachToGridOrMap(observer); // To make it possible to use when user is inside a locker/bag
        _mind.Visit(mind, observer, mindComp);

        args.Handled = true;
    }

    private void OnCyberVisionReturn(Entity<CyberdeckProjectionComponent> ent, ref CyberdeckVisionReturnEvent args)
    {
        if (args.Handled)
            return;

        ShutdownProjection(ent);
        args.Handled = true;
    }
}
