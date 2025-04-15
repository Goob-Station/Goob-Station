using Content.Shared.Charges.Components;
using Content.Shared.Charges.Systems;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Storage;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using System.Linq;

namespace Content.Shared._DV.Holosign;

public sealed class ChargeHolosignSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedChargesSystem _charges = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private HashSet<Entity<IComponent>> _signs = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChargeHolosignProjectorComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<ChargeHolosignProjectorComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ChargeHolosignProjectorComponent, BeforeRangedInteractEvent>(OnBeforeInteract);
    }

    private void OnInit(Entity<ChargeHolosignProjectorComponent> ent, ref ComponentInit args)
    {
        // its required, funny test is still funny
        if (string.IsNullOrEmpty(ent.Comp.SignComponentName))
            return;

        ent.Comp.Container = _container.EnsureContainer<Container>(ent, ent.Comp.ContainerId);
        ent.Comp.SignComponent = EntityManager.ComponentFactory.GetRegistration(ent.Comp.SignComponentName).Type;
    }

    private void OnMapInit(Entity<ChargeHolosignProjectorComponent> ent, ref MapInitEvent args)
    {
        if (!TryComp<LimitedChargesComponent>(ent, out var charges))
            return;

        var containers = Comp<ContainerManagerComponent>(ent);
        for (var i = 0; i < charges.MaxCharges; i++)
        {
            if (!TrySpawnInContainer(ent.Comp.SignProto, ent, ent.Comp.ContainerId, out _))
            {
                Log.Error($"Failed to spawn sign {ent.Comp.SignProto} for {ToPrettyString(ent)}!");
                return;
            }
        }
    }

    private void OnBeforeInteract(Entity<ChargeHolosignProjectorComponent> ent, ref BeforeRangedInteractEvent args)
    {
        if (args.Handled || !args.CanReach ||
            HasComp<StorageComponent>(args.Target) || // if it's a storage component like a bag, we ignore usage so it can be stored
            !TryComp<LimitedChargesComponent>(ent, out var charges))
            return;

        // first check if there's any existing holofans to clear
        var coords = args.ClickLocation.SnapToGrid(EntityManager);
        var mapCoords = _transform.ToMapCoordinates(coords);
        _signs.Clear();
        _lookup.GetEntitiesInRange(ent.Comp.SignComponent, mapCoords, 0.25f, _signs);
        if (_signs.Count == 0)
            TryPlaceSign((ent, ent, charges), coords, args.User);
        else
            TryRemoveSign((ent, ent, charges), _signs.First(), args.User);

        args.Handled = true;
    }

    public bool TryPlaceSign(Entity<ChargeHolosignProjectorComponent, LimitedChargesComponent> ent, EntityCoordinates coords, EntityUid user)
    {
        var container = ent.Comp1.Container;
        if (container.Count == 0 || !_charges.TryUseCharge((ent, ent.Comp2)))
        {
            _popup.PopupClient(Loc.GetString("charge-holoprojector-no-charges", ("item", ent)), ent, user);
            return false;
        }

        var placed = container.ContainedEntities.First(); // checked Count beforehand so this won't fail
        _transform.SetCoordinates(placed, coords);
        _transform.AnchorEntity(placed);
        return true;
    }

    public bool TryRemoveSign(Entity<ChargeHolosignProjectorComponent, LimitedChargesComponent> ent, EntityUid sign, EntityUid user)
    {
        // don't overfill
        if (ent.Comp2.Charges >= ent.Comp2.MaxCharges)
        {
            _popup.PopupClient(Loc.GetString("charge-holoprojector-charges-full", ("item", ent)), sign, user);
            return false;
        }

        if (!_container.Insert(sign, ent.Comp1.Container, force: true))
        {
            Log.Error($"Failed to insert holosign {ToPrettyString(sign)} back into {ToPrettyString(ent)}!");
            return false;
        }

        _charges.AddCharges(ent, 1, ent);

        var userIdentity = Identity.Name(user, EntityManager);
        _popup.PopupPredicted(
            Loc.GetString("charge-holoprojector-reclaim", ("sign", sign)),
            Loc.GetString("charge-holoprojector-reclaim-others", ("sign", sign), ("user", userIdentity)),
            ent,
            user);
        return true;
    }
}
