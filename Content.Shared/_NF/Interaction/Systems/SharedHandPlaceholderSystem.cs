using Content.Shared._NF.Interaction.Components;
using Content.Shared.Hands;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Item;
using Content.Shared.Whitelist;
using JetBrains.Annotations;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Shared._NF.Interaction.Systems;

/// <summary>
/// Handles interactions with items that spawn HandPlaceholder items.
/// </summary>
[UsedImplicitly]
public sealed partial class HandPlaceholderSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedItemSystem _item = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly MetaDataSystem _metadata = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<HandPlaceholderRemoveableComponent, GotUnequippedHandEvent>(OnUnequipHand);
        SubscribeLocalEvent<HandPlaceholderRemoveableComponent, DroppedEvent>(OnDropped);

        SubscribeLocalEvent<HandPlaceholderComponent, AfterInteractEvent>(AfterInteract);
        SubscribeLocalEvent<HandPlaceholderComponent, BeforeRangedInteractEvent>(BeforeRangedInteract);
    }

    private void OnUnequipHand(Entity<HandPlaceholderRemoveableComponent> ent, ref GotUnequippedHandEvent args)
    {
        if (args.Handled)
            return; // If this is happening in practice, this is a bug.

        SpawnAndPickUpPlaceholder(ent, args.User);
        RemCompDeferred<HandPlaceholderRemoveableComponent>(ent);
        args.Handled = true;
    }

    private void OnDropped(Entity<HandPlaceholderRemoveableComponent> ent, ref DroppedEvent args)
    {
        if (args.Handled)
            return; // If this is happening in practice, this is a bug.

        SpawnAndPickUpPlaceholder(ent, args.User);
        RemCompDeferred<HandPlaceholderRemoveableComponent>(ent);
        args.Handled = true;
    }

    private void SpawnAndPickUpPlaceholder(Entity<HandPlaceholderRemoveableComponent> ent, EntityUid user)
    {
        if (_net.IsServer)
        {
            var placeholder = Spawn("HandPlaceholder");
            if (TryComp<HandPlaceholderComponent>(placeholder, out var placeComp))
            {
                placeComp.Whitelist = ent.Comp.Whitelist;
                placeComp.Prototype = ent.Comp.Prototype;
                Dirty(placeholder, placeComp);
            }

            if (_proto.TryIndex(ent.Comp.Prototype, out var itemProto))
                _metadata.SetEntityName(placeholder, itemProto.Name);

            if (!_hands.TryPickup(user, placeholder)) // Can we get the hand this came from?
                QueueDel(placeholder);
        }
    }

    private void BeforeRangedInteract(Entity<HandPlaceholderComponent> ent, ref BeforeRangedInteractEvent args)
    {
        if (args.Target == null || args.Handled)
            return;

        args.Handled = true;
        TryToPickUpTarget(ent, args.Target.Value, args.User);
    }

    private void AfterInteract(Entity<HandPlaceholderComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Target == null || args.Handled)
            return;

        args.Handled = true;
        TryToPickUpTarget(ent, args.Target.Value, args.User);
    }

    private void TryToPickUpTarget(Entity<HandPlaceholderComponent> ent, EntityUid target, EntityUid user)
    {
        if (_whitelist.IsWhitelistFail(ent.Comp.Whitelist, target))
            return;

        // Can't get the hand we're holding this with? Something's wrong, abort.  No empty hands.
        if (!_hands.IsHolding(user, ent, out var hand))
            return;

        // Cache the whitelist/prototype, entity might be deleted.
        var whitelist = ent.Comp.Whitelist;
        var prototype = ent.Comp.Prototype;

        if (_net.IsServer)
            Del(ent);

        _hands.DoPickup(user, hand, target); // Force pickup - empty hands are not okay
        var placeComp = EnsureComp<HandPlaceholderRemoveableComponent>(target);
        placeComp.Whitelist = whitelist;
        placeComp.Prototype = prototype;
        Dirty(target, placeComp);
    }
}
