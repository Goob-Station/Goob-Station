using Content.Goobstation.Shared.CloneProjector.Clone;
using Content.Goobstation.Shared.Clothing.Components;
using Content.Shared.Actions;
using Content.Shared.Clothing;
using Content.Shared.Damage;
using Content.Shared.Humanoid;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Rejuvenate;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.CloneProjector;

public sealed class SharedCloneProjectorSystem : EntitySystem
{
    [Dependency] private readonly PrototypeManager _protoManager = default!;
    [Dependency] private readonly SharedHumanoidAppearanceSystem _humanoidAppearance = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly NetManager _net = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CloneProjectorComponent, MapInitEvent>(OnInit);

        SubscribeLocalEvent<CloneProjectorComponent, GetItemActionsEvent>(OnEquipped);
        SubscribeLocalEvent<CloneProjectorComponent, GotUnequippedEvent>(OnUnequipped);

        SubscribeLocalEvent<CloneProjectorComponent, CloneProjectorActivatedEvent>(OnProjectorActivated);

        SubscribeLocalEvent<CloneComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    private void OnInit(Entity<CloneProjectorComponent> projector, ref MapInitEvent args)
    {
        projector.Comp.CloneContainer = _container.EnsureContainer<Container>(projector.Owner, "CloneContainer");
    }

    private void OnEquipped(Entity<CloneProjectorComponent> projector, ref GetItemActionsEvent args)
    {
        if (args.InHands)
            return;

        args.AddAction(ref projector.Comp.ActionEntity, projector.Comp.Action);
    }

    private void OnUnequipped(Entity<CloneProjectorComponent> projector, ref GotUnequippedEvent args)
    {
        _actions.RemoveProvidedActions(args.Equipee, projector);
    }
    private void OnProjectorActivated(Entity<CloneProjectorComponent> projector, ref CloneProjectorActivatedEvent args)
    {
        if (args.Handled
            || _net.IsClient)
            return;

        if (projector.Comp.CloneUid != null)
        {
            TryInsertClone(projector);
            return;
        }

        if (projector.Comp.CloneUid != args.Performer)
        {
            if (TrySpawnClone(projector, args.Performer))
            {
                args.Handled = true;
                return;
            }
        }

        if (!TryRetrieveClone(projector))
            return;

        args.Handled = true;
    }

    private void OnMobStateChanged(Entity<CloneComponent> clone, ref MobStateChangedEvent args)
    {
        if (!_mobState.IsIncapacitated(clone)
            || clone.Comp.HostProjector is not { } projector)
            return;

        TryInsertClone(projector);
        RaiseLocalEvent(clone, new RejuvenateEvent(true, false));

        // add cooldown here
    }

    private bool TrySpawnClone(CloneProjectorComponent projector, EntityUid performer)
    {
        if (!TryComp<HumanoidAppearanceComponent>(performer, out var appearance))
            return false;

        _container.CleanContainer(projector.CloneContainer);

        var speciesId = appearance.Species;

        if (!_protoManager.TryIndex(speciesId, out var species))
            return false;

        var clone = Spawn(species.Prototype, Transform(performer).Coordinates);
        _humanoidAppearance.CloneAppearance(performer, clone);

        if (projector.AddedComponents != null)
            EntityManager.AddComponents(clone, projector.AddedComponents);

        if (projector.RemovedComponents != null)
            EntityManager.RemoveComponents(clone, projector.RemovedComponents);

        var cloneComp = EnsureComp<CloneComponent>(clone);

        cloneComp.HostProjector = projector;
        cloneComp.HostEntity = performer;

        _damageable.SetDamageModifierSetId(clone, projector.CloneDamageModifierSet);
        projector.CloneUid = clone;

        if (!TryComp<InventoryComponent>(clone, out var inventoryComponent))
            return false;

        _inventory.SetTemplateId((clone, inventoryComponent), projector.CloneInventoryTemplate);

        return true;
    }

    private bool TryInsertClone(CloneProjectorComponent projector)
    {
        if (projector.CloneUid is not { } clone)
            return false;

        _container.Insert(clone, projector.CloneContainer);
        projector.IsActive = false;

        return true;
    }

    private bool TryRetrieveClone(Entity<CloneProjectorComponent> projector)
    {
        return projector.Comp.CloneUid is { } clone
               && _container.TryRemoveFromContainer(clone);
    }
}
