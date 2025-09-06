using System.Collections.Generic;
using Content.Goobstation.Shared.FloorGoblin;
using Content.Shared.Actions;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Mobs;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;

namespace Content.Goobstation.Server.FloorGoblin;

public sealed partial class StealShoesSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedContainerSystem _containers = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<StealShoesComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<StealShoesComponent, StealShoesEvent>(OnStealShoes);
        SubscribeLocalEvent<StealShoesComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    private void OnInit(EntityUid uid, StealShoesComponent component, ComponentInit args)
    {
        if (component.StealAction == null)
            _actions.AddAction(uid, ref component.StealAction, component.ActionProto);
        _containers.EnsureContainer<Container>(uid, component.ContainerId);
    }

    private void OnStealShoes(EntityUid uid, StealShoesComponent component, StealShoesEvent args)
    {
        if (args.Handled)
            return;

        var target = args.Target;

        if (!_interaction.InRangeUnobstructed(uid, target))
            return;

        EntityUid? shoesUid;
        if (!_inventory.TryGetSlotEntity(target, "shoes", out shoesUid) || shoesUid == null)
        {
            _popup.PopupEntity("No shoes to steal.", uid);
            return;
        }

        if (!_inventory.TryUnequip(target, "shoes"))
        {
            _popup.PopupEntity("No shoes to steal.", uid);
            return;
        }

        if (!_containers.TryGetContainer(uid, component.ContainerId, out var container))
            container = _containers.EnsureContainer<Container>(uid, component.ContainerId);

        _containers.Insert(shoesUid.Value, container);

        if (component.ChompSound != null)
            _audio.PlayPvs(component.ChompSound, uid);

        _popup.PopupEntity("You snatch the shoes!", uid);
        _popup.PopupEntity("Your shoes are stolen!", target);
        args.Handled = true;
    }

    private void OnMobStateChanged(EntityUid uid, StealShoesComponent component, MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        if (_containers.TryGetContainer(uid, component.ContainerId, out var container))
        {
            var dropCoords = Transform(uid).Coordinates;
            var toDrop = new List<EntityUid>(container.ContainedEntities);
            foreach (var ent in toDrop)
            {
                _containers.Remove(ent, container);
                _xform.SetCoordinates(ent, dropCoords);
            }
        }
    }
}
