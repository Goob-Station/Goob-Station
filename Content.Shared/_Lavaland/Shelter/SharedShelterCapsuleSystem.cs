using System.Linq;
using Content.Shared.DoAfter;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Robust.Shared.Map.Components;

namespace Content.Shared._Lavaland.Shelter;

public abstract class SharedShelterCapsuleSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShelterCapsuleComponent, UseInHandEvent>(OnAfterInteract);
    }

    private void OnAfterInteract(EntityUid uid, ShelterCapsuleComponent component, UseInHandEvent args)
    {
        if (args.Handled)
            return;

        var doAfterEventArgs = new DoAfterArgs(EntityManager, args.User, component.DeployTime, new ShelterCapsuleDeployDoAfterEvent(), uid, used: uid)
        {
            BreakOnMove = true,
            NeedHand = true,
        };

        if (!CheckCanDeploy((uid, component)))
        {
            args.Handled = true;
            return;
        }

        _doAfterSystem.TryStartDoAfter(doAfterEventArgs);
        args.Handled = true;
    }

    protected bool CheckCanDeploy(Entity<ShelterCapsuleComponent> ent)
    {
        var xform = Transform(ent);
        var comp = ent.Comp;

        // Works only on planets!
        if (xform.GridUid == null || xform.MapUid == null || xform.GridUid != xform.MapUid || !TryComp<MapGridComponent>(xform.GridUid.Value, out var gridComp))
        {
            _popup.PopupCoordinates(Loc.GetString("shelter-capsule-fail-no-planet"), xform.Coordinates);
            return false;
        }

        var worldPos = _transform.GetMapCoordinates(ent, xform);

        // Make sure that surrounding area does not have any entities with physics
        var box = Box2.CenteredAround(worldPos.Position.Rounded(), comp.BoxSize);

        // Doesn't work near other grids
        if (_lookup.GetEntitiesInRange<MapGridComponent>(xform.Coordinates, comp.BoxSize.Length()).Any())
        {
            _popup.PopupCoordinates(Loc.GetString("shelter-capsule-fail-near-grid"), xform.Coordinates);
            return false;
        }

        if (_mapSystem.GetAnchoredEntities(xform.GridUid.Value, gridComp, box).Any())
        {
            _popup.PopupCoordinates(Loc.GetString("shelter-capsule-fail-no-space"), xform.Coordinates);
            return false;
        }

        return true;
    }
}
