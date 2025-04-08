using Content.Shared._NF.Interaction.Components;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Item;
using Content.Shared.Whitelist;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._NF.Interaction.Systems;

/// <summary>
/// Handles interactions with items that swap with HandPlaceholder items.
/// </summary>
public sealed partial class HandPlaceholderSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly MetaDataSystem _metadata = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public readonly EntProtoId<HandPlaceholderComponent> Placeholder = "HandPlaceholder";

    public override void Initialize()
    {
        SubscribeLocalEvent<HandPlaceholderRemoveableComponent, EntGotRemovedFromContainerMessage>(OnEntityRemovedFromContainer);

        SubscribeLocalEvent<HandPlaceholderComponent, BeforeRangedInteractEvent>(BeforeRangedInteract);
        SubscribeLocalEvent<HandPlaceholderComponent, ContainerGettingRemovedAttemptEvent>(OnRemoveAttempt);
    }

    /// <summary>
    /// Spawns a new placeholder and ties it to an item.
    /// When dropped the item will replace itself with the placeholder in its container.
    /// </summary>
    public void SpawnPlaceholder(BaseContainer container, EntityUid item, EntProtoId id, EntityWhitelist whitelist)
    {
        var placeholder = Spawn(Placeholder);
        var comp = Comp<HandPlaceholderComponent>(placeholder);
        comp.Prototype = id;
        comp.Whitelist = whitelist;
        comp.Source = container.Owner;
        comp.ContainerId = container.ID;
        Dirty(placeholder, comp);

        var name = _proto.Index(id).Name;
        _metadata.SetEntityName(placeholder, name);
        SetPlaceholder(item, placeholder);

        var succeeded = _container.Insert(placeholder, container, force: true);
        DebugTools.Assert(succeeded, $"Failed to insert placeholder {ToPrettyString(placeholder)} into {ToPrettyString(comp.Source)}");
    }

    /// <summary>
    /// Sets the placeholder entity for an item.
    /// </summary>
    public void SetPlaceholder(EntityUid item, EntityUid placeholder)
    {
        var comp = EnsureComp<HandPlaceholderRemoveableComponent>(item);
        comp.Placeholder = placeholder;
        Dirty(item, comp);
    }

    public void SetEnabled(EntityUid item, bool enabled)
    {
        if (TryComp<HandPlaceholderRemoveableComponent>(item, out var comp))
        {
            comp.Enabled = enabled;
            Dirty(item, comp);
        }
        else if (TryComp<HandPlaceholderComponent>(item, out var placeholder))
        {
            placeholder.Enabled = enabled;
            Dirty(item, placeholder);
        }
    }

    private void SwapPlaceholder(Entity<HandPlaceholderRemoveableComponent> ent, BaseContainer container)
    {
        // trying to insert when deleted is an error, and only handle when it is being actually dropped
        var owner = container.Owner;
        if (!ent.Comp.Enabled || TerminatingOrDeleted(owner))
            return;

        var placeholder = ent.Comp.Placeholder;

        ent.Comp.Enabled = false;
        RemCompDeferred<HandPlaceholderRemoveableComponent>(ent);

        // stop tests failing
        if (TerminatingOrDeleted(placeholder))
            return;

        SetEnabled(placeholder, false);
        var succeeded = _container.Insert(placeholder, container, force: true);
        DebugTools.Assert(succeeded, $"Failed to insert placeholder {ToPrettyString(placeholder)} of {ToPrettyString(ent)} into container of {ToPrettyString(owner)}");
        SetEnabled(placeholder, true); // prevent dropping it now that it's in hand
    }

    private void OnEntityRemovedFromContainer(Entity<HandPlaceholderRemoveableComponent> ent, ref EntGotRemovedFromContainerMessage args)
    {
        SwapPlaceholder(ent, args.Container);
    }

    private void BeforeRangedInteract(Entity<HandPlaceholderComponent> ent, ref BeforeRangedInteractEvent args)
    {
        if (args.Handled || !args.CanReach || args.Target is not { } target)
            return;

        args.Handled = true;
        TryToPickUpTarget(ent, target, args.User);
    }

    private void OnRemoveAttempt(Entity<HandPlaceholderComponent> ent, ref ContainerGettingRemovedAttemptEvent args)
    {
        if (ent.Comp.Enabled)
            args.Cancel();
    }

    private void TryToPickUpTarget(Entity<HandPlaceholderComponent> ent, EntityUid target, EntityUid user)
    {
        // require items regardless of the whitelist
        if (!HasComp<ItemComponent>(target) || _whitelist.IsWhitelistFail(ent.Comp.Whitelist, target))
            return;

        if (!TryComp<HandsComponent>(user, out var hands))
            return;

        // Can't get the hand we're holding this with? Something's wrong, abort.  No empty hands.
        if (!_hands.IsHolding(user, ent, out var hand, hands))
            return;

        SetPlaceholder(target, ent);
        SetEnabled(target, true);

        SetEnabled(ent, false); // allow inserting into the source container

        if (ent.Comp.Source is { } source)
        {
            var container = _container.GetContainer(source, ent.Comp.ContainerId);
            var succeeded = _container.Insert(ent.Owner, container, force: true);
            DebugTools.Assert(succeeded, $"Failed to insert {ToPrettyString(ent)} into {container.ID} of {ToPrettyString(source)}");
        }
        else
        {
            Log.Error($"Placeholder {ToPrettyString(ent)} had no source set");
        }

        _hands.DoPickup(user, hand, target, hands); // Force pickup - empty hands are not okay
        _interaction.DoContactInteraction(user, target); // allow for forensics and other systems to work (why does hands system not do this???)
    }
}