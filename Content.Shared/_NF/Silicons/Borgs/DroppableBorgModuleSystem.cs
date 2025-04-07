using Content.Shared._NF.Interaction.Systems;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Silicons.Borgs.Components;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Utility;

namespace Content.Shared._NF.Silicons.Borgs;

public sealed class DroppableBorgModuleSystem : EntitySystem
{
    [Dependency] private readonly HandPlaceholderSystem _placeholder = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DroppableBorgModuleComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<DroppableBorgModuleComponent, BorgCanInsertModuleEvent>(OnCanInsertModule);
        SubscribeLocalEvent<DroppableBorgModuleComponent, BorgModuleSelectedEvent>(OnModuleSelected);
        SubscribeLocalEvent<DroppableBorgModuleComponent, BorgModuleUnselectedEvent>(OnModuleUnselected);
    }

    private void OnMapInit(Entity<DroppableBorgModuleComponent> ent, ref MapInitEvent args)
    {
        _container.EnsureContainer<Container>(ent, ent.Comp.ContainerId);
        var placeholders = _container.EnsureContainer<Container>(ent, ent.Comp.PlaceholderContainerId);

        foreach (var slot in ent.Comp.Items)
        {
            // only the server runs mapinit, this wont make clientside entities
            var successful = TrySpawnInContainer(slot.Id, ent, ent.Comp.ContainerId, out var item);
            // this would only fail if the current entity is being terminated, which is impossible for mapinit
            DebugTools.Assert(successful, $"Somehow failed to insert {ToPrettyString(item)} into {ToPrettyString(ent)}");
            _placeholder.SpawnPlaceholder(placeholders, item!.Value, slot.Id, slot.Whitelist);
        }

        Dirty(ent);
    }

    private void OnCanInsertModule(Entity<DroppableBorgModuleComponent> ent, ref BorgCanInsertModuleEvent args)
    {
        if (args.Cancelled)
            return;

        foreach (var module in args.Chassis.Comp.ModuleContainer.ContainedEntities)
        {
            if (!TryComp<DroppableBorgModuleComponent>(module, out var comp))
                continue;

            if (comp.ModuleId != ent.Comp.ModuleId)
                continue;

            if (args.User is { } user)
                _popup.PopupEntity(Loc.GetString("borg-module-duplicate"), args.Chassis, user); // event is only raised by server so not using PopupClient
            args.Cancelled = true;
            return;
        }
    }

    private void OnModuleSelected(Entity<DroppableBorgModuleComponent> ent, ref BorgModuleSelectedEvent args)
    {
        var chassis = args.Chassis;
        if (!TryComp<HandsComponent>(chassis, out var hands))
            return;

        var container = _container.GetContainer(ent, ent.Comp.ContainerId);
        var items = container.ContainedEntities;
        for (int i = 0; i < ent.Comp.Items.Count; i++)
        {
            var item = items[0]; // the contained items will gradually go to 0
            var handId = HandId(ent, i);
            _hands.AddHand(chassis, handId, HandLocation.Middle, hands);
            var hand = hands.Hands[handId];
            _hands.DoPickup(chassis, hand, item, hands);
            if (hand.HeldEntity != item)
            {
                Log.Error($"Failed to pick up {ToPrettyString(item)} into hand {handId} of {ToPrettyString(chassis)}, it holds {ToPrettyString(hand.HeldEntity)}");
                // If we didn't pick up our expected item, delete the hand.  No free hands!
                _hands.RemoveHand(chassis, handId, hands);
            }
            else
            {
                _interaction.DoContactInteraction(chassis, item); // for potential forensics or other systems (why does hands system not do this)
                _placeholder.SetEnabled(item, true);
            }
        }
    }

    private void OnModuleUnselected(Entity<DroppableBorgModuleComponent> ent, ref BorgModuleUnselectedEvent args)
    {
        var chassis = args.Chassis;
        if (!TryComp<HandsComponent>(chassis, out var hands))
            return;

        if (TerminatingOrDeleted(ent))
        {
            for (int i = 0; i < ent.Comp.Items.Count; i++)
            {
                var handId = HandId(ent, i);
                _hands.TryGetHand(chassis, handId, out var hand, hands);
                if (hand?.HeldEntity is { } item)
                    QueueDel(item);
                else if (!TerminatingOrDeleted(chassis)) // don't care if its empty if the server is shutting down
                    Log.Error($"Borg {ToPrettyString(chassis)} terminated with empty hand {i} in {ToPrettyString(ent)}");
                _hands.RemoveHand(chassis, handId, hands);
            }
            return;
        }

        var container = _container.GetContainer(ent, ent.Comp.ContainerId);
        for (int i = 0; i < ent.Comp.Items.Count; i++)
        {
            var handId = HandId(ent, i);
            _hands.TryGetHand(chassis, handId, out var hand, hands);
            if (hand?.HeldEntity is { } item)
            {
                _placeholder.SetEnabled(item, false);
                _container.Insert(item, container, force: true);
            }
            else
            {
                Log.Error($"Borg {ToPrettyString(chassis)} had an empty hand in the slot for {ent.Comp.Items[i].Id}");
            }

            _hands.RemoveHand(chassis, handId, hands);
        }
    }

    /// <summary>
    /// Format the hand ID for a given module and item number.
    /// </summary>
    private static string HandId(EntityUid uid, int i)
    {
        return $"nf-{uid}-item-{i}";
    }
}

/// <summary>
/// Event raised on a module to check if it can be installed.
/// This should exist upstream but doesn't.
/// </summary>
[ByRefEvent]
public record struct BorgCanInsertModuleEvent(Entity<BorgChassisComponent> Chassis, EntityUid? User, bool Cancelled = false);
