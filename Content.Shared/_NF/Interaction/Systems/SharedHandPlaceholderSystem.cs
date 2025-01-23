using Content.Shared._NF.Interaction.Components;
using Content.Shared.Hands;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction.Events;
using Content.Shared.Whitelist;
using JetBrains.Annotations;
using Robust.Shared.Network;

namespace Content.Shared._NF.Interaction.Systems;

/// <summary>
/// Handles interactions with items that spawn HandPlaceholder items.
/// </summary>
[UsedImplicitly]
public abstract class SharedHandPlaceholderSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<HandPlaceholderRemoveableComponent, GotUnequippedHandEvent>(OnUnequipHand);
        SubscribeLocalEvent<HandPlaceholderRemoveableComponent, DroppedEvent>(OnDropped);
    }

    private void OnUnequipHand(Entity<HandPlaceholderRemoveableComponent> ent, ref GotUnequippedHandEvent args)
    {
        if (args.Handled)
            return; // If this is happening in practice, this is a bug.

        if (_net.IsServer)
        {
            var placeholder = Spawn("HandPlaceholder");
            if (TryComp<HandPlaceholderComponent>(placeholder, out var placeComp))
                placeComp.Whitelist = ent.Comp.Whitelist;
            if (!_hands.TryPickup(args.User, placeholder, args.Hand.Name)) // Can anyone other than borgs unequip their module items?
                QueueDel(placeholder);
        }
        RemCompDeferred<HandPlaceholderRemoveableComponent>(ent);
        args.Handled = true;
    }

    private void OnDropped(Entity<HandPlaceholderRemoveableComponent> ent, ref DroppedEvent args)
    {
        if (args.Handled)
            return; // If this is happening in practice, this is a bug.

        RemCompDeferred<HandPlaceholderRemoveableComponent>(ent);
        if (_net.IsServer)
        {
            var placeholder = Spawn("HandPlaceholder");
            if (TryComp<HandPlaceholderComponent>(placeholder, out var placeComp))
                placeComp.Whitelist = ent.Comp.Whitelist;

            if (!_hands.TryPickup(args.User, placeholder)) // Can we get the hand this came from?
                QueueDel(placeholder);
        }
        args.Handled = true;
    }
}
