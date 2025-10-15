// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Charges.Components;
using Content.Shared.Charges.Systems;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared.Storage;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Timing;
using System.Linq;

namespace Content.Shared._DV.Holosign;

public sealed class ChargeHolosignSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;
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
        SubscribeLocalEvent<ChargeHolosignProjectorComponent, UseInHandEvent>(OnUseInHand);
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
            if (!TrySpawnInContainer(ent.Comp.SignProto, ent, ent.Comp.ContainerId, out var signUid))
            {
                Log.Error($"Failed to spawn sign {ent.Comp.SignProto} for {ToPrettyString(ent)}!");
                return;
            }

            ent.Comp.Signs.Add(signUid.Value);
        }

        DirtyField(ent, ent.Comp, nameof(ChargeHolosignProjectorComponent.Signs));
    }

    private void OnBeforeInteract(Entity<ChargeHolosignProjectorComponent> ent, ref BeforeRangedInteractEvent args)
    {
        if (!_timing.IsFirstTimePredicted ||
            args.Handled || !args.CanReach ||
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

    private void OnUseInHand(Entity<ChargeHolosignProjectorComponent> ent, ref UseInHandEvent args)
    {
        if (!_timing.IsFirstTimePredicted || !TryComp<LimitedChargesComponent>(ent, out var charges))
            return;

        // count how many holosigns we actually managed to recall
        var count = 0;
        var remQueue = new List<EntityUid>();

        // recall all holosigns we can
        foreach (var signUid in ent.Comp.Signs)
        {
            if (TerminatingOrDeleted(signUid))
            {
                remQueue.Add(signUid);
                continue;
            }

            if (ent.Comp.Container.Contains(signUid) || TryRemoveSign((ent, ent.Comp, charges), signUid, args.User, false))
            {
                count++;
            }
            else
            {
                // delete it if we can't recall it
                if (_net.IsServer)
                    QueueDel(signUid);
                remQueue.Add(signUid);
            }
        }

        foreach (var signUid in remQueue)
            ent.Comp.Signs.Remove(signUid);

        // spawn replacements for holosigns we couldn't recall
        for (var i = count; i < charges.MaxCharges; i++)
        {
            if (!TrySpawnInContainer(ent.Comp.SignProto, ent, ent.Comp.ContainerId, out var signUid))
            {
                Log.Error($"Failed to spawn sign {ent.Comp.SignProto} for {ToPrettyString(ent)}!");
                break;
            }

            _charges.AddCharges((ent, charges), 1);
            ent.Comp.Signs.Add(signUid.Value);
        }

        DirtyField(ent, ent.Comp, nameof(ChargeHolosignProjectorComponent.Signs));
    }

    public bool TryPlaceSign(Entity<ChargeHolosignProjectorComponent?, LimitedChargesComponent?> ent, EntityCoordinates coords, EntityUid user)
    {
        if (!Resolve(ent, ref ent.Comp1, ref ent.Comp2))
            return false;

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

    public bool TryRemoveSign(Entity<ChargeHolosignProjectorComponent?, LimitedChargesComponent?> ent, EntityUid sign, EntityUid user, bool showIdentity = true)
    {
        if (!Resolve(ent, ref ent.Comp1, ref ent.Comp2))
            return false;

        // don't overfill
        if (_charges.GetCurrentCharges((ent, ent.Comp2)) >= ent.Comp2.MaxCharges)
        {
            _popup.PopupClient(Loc.GetString("charge-holoprojector-charges-full", ("item", ent)), sign, user);
            return false;
        }

        if (!_container.Insert(sign, ent.Comp1.Container, force: true))
        {
            Log.Error($"Failed to insert holosign {ToPrettyString(sign)} back into {ToPrettyString(ent)}!");
            return false;
        }

        _charges.AddCharges((ent, ent.Comp2), 1);

        var othersStr = showIdentity ? Loc.GetString("charge-holoprojector-reclaim-others", ("sign", sign), ("user", Identity.Name(user, EntityManager)))
                                     : Loc.GetString("charge-holoprojector-recall-others", ("sign", sign));
        _popup.PopupPredicted(
            Loc.GetString("charge-holoprojector-reclaim", ("sign", sign)),
            othersStr,
            ent,
            user);
        return true;
    }
}
