using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.GameObjects;
using Robust.Shared.Timing;

namespace Content.Shared._DV.SmartFridge;

public sealed class SmartFridgeSystem : EntitySystem
{
    [Dependency] private readonly AccessReaderSystem _accessReader = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IGameTiming _timing = default!; // Frontier

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SmartFridgeComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<SmartFridgeComponent, EntRemovedFromContainerMessage>(OnItemRemoved);

        Subs.BuiEvents<SmartFridgeComponent>(SmartFridgeUiKey.Key,
            sub =>
            {
                sub.Event<SmartFridgeDispenseItemMessage>(OnDispenseItem);
            });
    }

    private void OnInteractUsing(Entity<SmartFridgeComponent> ent, ref InteractUsingEvent args)
    {
        if (!_container.TryGetContainer(ent, ent.Comp.Container, out var container))
            return;

        if (_whitelist.IsWhitelistFail(ent.Comp.Whitelist, args.Used) || _whitelist.IsBlacklistPass(ent.Comp.Blacklist, args.Used))
            return;

        if (!Allowed(ent, args.User))
            return;

        if (container.Count >= ent.Comp.MaxContainedCount) // Frontier
            return; // Frontier

        if (!_hands.TryDrop(args.User, args.Used))
            return;

        _audio.PlayPredicted(ent.Comp.InsertSound, ent, args.User);
        _container.Insert(args.Used, container);
        var key = new SmartFridgeEntry(Identity.Name(args.Used, EntityManager));
        if (!ent.Comp.Entries.Contains(key))
            ent.Comp.Entries.Add(key);
        ent.Comp.ContainedEntries.TryAdd(key, new());
        var entries = ent.Comp.ContainedEntries[key];
        if (!entries.Contains(GetNetEntity(args.Used)))
            entries.Add(GetNetEntity(args.Used));
        Dirty(ent);
    }

    private void OnItemRemoved(Entity<SmartFridgeComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        var key = new SmartFridgeEntry(Identity.Name(args.Entity, EntityManager));

        if (ent.Comp.ContainedEntries.TryGetValue(key, out var contained))
        {
            contained.Remove(GetNetEntity(args.Entity));
            // Frontier: remove listing when empty
            if (contained.Count <= 0)
            {
                ent.Comp.ContainedEntries.Remove(key);
                ent.Comp.Entries.Remove(key);
            }
            // End Frontier: remove listing when empty
        }

        Dirty(ent);
    }

    private bool Allowed(Entity<SmartFridgeComponent> machine, EntityUid user)
    {
        if (_accessReader.IsAllowed(user, machine))
            return true;

        _popup.PopupPredicted(Loc.GetString("smart-fridge-component-try-eject-access-denied"), machine, user);
        _audio.PlayPredicted(machine.Comp.SoundDeny, machine, user);
        return false;
    }

    private void OnDispenseItem(Entity<SmartFridgeComponent> ent, ref SmartFridgeDispenseItemMessage args)
    {
        if (!_timing.IsFirstTimePredicted) // Frontier: less prediction jank in the UI
            return; // Frontier

        if (!Allowed(ent, args.Actor))
            return;

        if (!ent.Comp.ContainedEntries.TryGetValue(args.Entry, out var contained))
        {
            _audio.PlayPredicted(ent.Comp.SoundDeny, ent, args.Actor);
            _popup.PopupPredicted(Loc.GetString("smart-fridge-component-try-eject-unknown-entry"), ent, args.Actor);
            return;
        }

        foreach (var item in contained)
        {
            if (!_container.TryRemoveFromContainer(GetEntity(item)))
                continue;

            _audio.PlayPredicted(ent.Comp.SoundVend, ent, args.Actor);
            contained.Remove(item);
            // Frontier: remove listing when empty
            if (contained.Count <= 0)
            {
                ent.Comp.ContainedEntries.Remove(args.Entry);
                ent.Comp.Entries.Remove(args.Entry);
            }
            // End Frontier: remove listing when empty
            Dirty(ent);
            return;
        }

        _audio.PlayPredicted(ent.Comp.SoundDeny, ent, args.Actor);
        _popup.PopupPredicted(Loc.GetString("smart-fridge-component-try-eject-out-of-stock"), ent, args.Actor);
    }

    // Frontier: hacky function to insert an object
    public bool TryInsertObject(Entity<SmartFridgeComponent> ent, EntityUid item, EntityUid? user)
    {
        if (!_container.TryGetContainer(ent, ent.Comp.Container, out var container))
            return false;

        if (_whitelist.IsWhitelistFail(ent.Comp.Whitelist, item) || _whitelist.IsBlacklistPass(ent.Comp.Blacklist, item))
            return false;

        if (user is { Valid: true } userUid && !Allowed(ent, userUid))
            return false;

        if (container.Count >= ent.Comp.MaxContainedCount)
            return false;

        _audio.PlayPredicted(ent.Comp.InsertSound, ent, user);
        _container.Insert(item, container);
        var key = new SmartFridgeEntry(Identity.Name(item, EntityManager));
        if (!ent.Comp.Entries.Contains(key))
            ent.Comp.Entries.Add(key);
        ent.Comp.ContainedEntries.TryAdd(key, new());
        var entries = ent.Comp.ContainedEntries[key];
        if (!entries.Contains(GetNetEntity(item)))
            entries.Add(GetNetEntity(item));
        Dirty(ent);
        return true;
    }
    // End Frontier: hacky function to insert an object
}
