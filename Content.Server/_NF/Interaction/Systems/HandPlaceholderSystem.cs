using Content.Shared._NF.Interaction.Components;
using Content.Shared._NF.Interaction.Systems;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Whitelist;
using JetBrains.Annotations;

namespace Content.Server._NF.Interaction.Systems;

/// <summary>
/// Handles interactions with items that spawn HandPlaceholder items.
/// </summary>
[UsedImplicitly]
public sealed partial class HandPlaceholderSystem : SharedHandPlaceholderSystem
{
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HandPlaceholderComponent, AfterInteractEvent>(AfterInteract);
        SubscribeLocalEvent<HandPlaceholderComponent, BeforeRangedInteractEvent>(BeforeRangedInteract);
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

        _hands.IsHolding(user, ent, out var hand); // Try to get the hand we're using
        var whitelist = ent.Comp.Whitelist; // Cache the whitelist, we're 
        Del(ent);
        if (_hands.TryPickup(user, target, hand?.Name)) // Try to replace the placeholder with the target item
        {
            var placeComp = EnsureComp<HandPlaceholderRemoveableComponent>(target);
            placeComp.Whitelist = whitelist;
        }
    }
}
