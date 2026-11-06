// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Maps;
using Content.Shared.Popups;
using Robust.Shared.Map.Components;

namespace Content.Goobstation.Server.Traps;

public sealed partial class TrapDeploySystem : EntitySystem
{
    [Dependency] private SharedPopupSystem _popups = default!;
    [Dependency] private SharedHandsSystem _hands = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private TurfSystem _turf = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TrapDeployComponent, UseInHandEvent>(OnDeployUse);
        SubscribeLocalEvent<TrapPickupComponent, ActivateInWorldEvent>(OnTrapActivate);
    }

    private void OnDeployUse(Entity<TrapDeployComponent> ent, ref UseInHandEvent args)
    {
        if (args.Handled)
            return;

        var coords = _transform.GetMoverCoordinates(args.User);

        if (TryComp<TransformComponent>(args.User, out var xform) &&
            TryComp<MapGridComponent>(xform.GridUid, out var grid))
        {
            var tile = _turf.GetTileRef(coords);
            if (tile is not { } tileRef || _turf.IsSpace(tileRef))
            {
                _popups.PopupClient(Loc.GetString("trap-deploy-cant-space"), ent.Owner, args.User, PopupType.MediumCaution);
                return;
            }
        }

        args.Handled = true;

        Spawn(ent.Comp.DeployedPrototype, coords);
        _popups.PopupClient(Loc.GetString("trap-deploy-placed"), ent.Owner, args.User);

        QueueDel(ent.Owner);
    }

    private void OnTrapActivate(Entity<TrapPickupComponent> ent, ref ActivateInWorldEvent args)
    {
        if (args.Handled || !args.Complex)
            return;

        args.Handled = true;

        var item = Spawn(ent.Comp.ItemPrototype, _transform.GetMoverCoordinates(ent.Owner));
        _hands.TryPickupAnyHand(args.User, item);
        QueueDel(ent.Owner);
    }
}
