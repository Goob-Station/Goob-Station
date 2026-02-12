// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Will-Oliver-Br <164823659+Will-Oliver-Br@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Access.Systems;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Timing;
using Content.Shared.DragDrop; // goob

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
        SubscribeLocalEvent<SmartFridgeComponent, CanDropTargetEvent>(OnCanDropTarget); // goob
        SubscribeLocalEvent<SmartFridgeComponent, DragDropTargetEvent>(OnDragDropTarget); // goob

        Subs.BuiEvents<SmartFridgeComponent>(SmartFridgeUiKey.Key,
            sub =>
            {
                sub.Event<SmartFridgeDispenseItemMessage>(OnDispenseItem);
            });
    }

    // Goobstation changes - Drag & Drop insert START
    /// <summary>
    /// Try to insert <paramref name="thingToInsert"/> into the smartfridge (<paramref name="ent"/>).
    /// </summary>
    /// <param name="ent">The smartfridge entity.</param>
    /// <param name="user">The user trying to insert something.</param>
    /// <param name="thingToInsert">The thing to be inserted into the smartfridge.</param>
    /// <returns>Returns <see langword="true"/> if the item was successfully inserted, and <see langword="false"/> otherwise.</returns>
    public bool TryInsertEntity(Entity<SmartFridgeComponent> ent, EntityUid user, EntityUid thingToInsert)
    {
        if (!CanInsertEntity(ent, user, thingToInsert))
            return false;
        if (_hands.IsHolding(user, thingToInsert, out var handId) && !_hands.TryDrop(user, handId))
            return false;

        InsertEntity(ent, user, thingToInsert);
        return true;
    }

    /// <summary>
    /// Check to see if <paramref name="user"/> is able to insert <paramref name="thingToInsert"/> into the smartfridge (<paramref name="ent"/>).
    /// </summary>
    /// <param name="ent">The smartfridge entity.</param>
    /// <param name="user">The user trying to insert something.</param>
    /// <param name="thingToInsert">The thing to be inserted into the smartfridge.</param>
    /// <returns>Returns <see langword="true"/> if the item can be inserted, and <see langword="false"/> otherwise.</returns>
    private bool CanInsertEntity(Entity<SmartFridgeComponent> ent, EntityUid user, EntityUid thingToInsert)
    {
        if (!_container.TryGetContainer(ent, ent.Comp.Container, out var container))
            return false;

        if (container.Count >= ent.Comp.MaxContainedCount)
            return false;

        if (_whitelist.IsWhitelistFail(ent.Comp.Whitelist, thingToInsert) || _whitelist.IsBlacklistPass(ent.Comp.Blacklist, thingToInsert))
            return false;

        if (!Allowed(ent, user))
            return false;

        return true;
    }

    /// <summary>
    /// Insert <paramref name="thingToInsert"/> into the smartfridge (<paramref name="ent"/>).
    /// </summary>
    /// <remarks>
    /// Please check <see cref="CanInsertEntity(Entity{SmartFridgeComponent}, EntityUid, EntityUid)">CanInsertEntity()</see> prior to calling this
    /// to make sure that it's actually possible to insert the entity.
    /// </remarks>
    /// <param name="ent">The smartfridge entity.</param>
    /// <param name="user">The user trying to insert something.</param>
    /// <param name="thingToInsert">The thing to be inserted into the smartfridge.</param>
    /// <exception cref="Exception">Thrown if <paramref name="ent"/> doesn't have a valid <see cref="BaseContainer"/> container.</exception>
    /// <seealso cref="TryInsertEntity(Entity{SmartFridgeComponent}, EntityUid, EntityUid)"/>
    private void InsertEntity(Entity<SmartFridgeComponent> ent, EntityUid user, EntityUid thingToInsert)
    {
        if (!_container.TryGetContainer(ent, ent.Comp.Container, out var container))
            throw new Exception("`InsertObject()` called on a smartfridge without a valid container.");

        _audio.PlayPredicted(ent.Comp.InsertSound, ent, user);
        _container.Insert(thingToInsert, container);

        var key = new SmartFridgeEntry(Identity.Name(thingToInsert, EntityManager));
        if (!ent.Comp.Entries.Contains(key))
            ent.Comp.Entries.Add(key);

        ent.Comp.ContainedEntries.TryAdd(key, new());
        var entries = ent.Comp.ContainedEntries[key];

        var thingNetEntity = GetNetEntity(thingToInsert);
        if (!entries.Contains(thingNetEntity))
            entries.Add(thingNetEntity);
        Dirty(ent);
    }

    private void OnInteractUsing(Entity<SmartFridgeComponent> ent, ref InteractUsingEvent args)
    {
        TryInsertEntity(ent, args.User, args.Used);
    }
    // Goobstation changes - Drag & Drop insert END

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

    // Goobstation changes - Drag & Drop insert START
    private void OnCanDropTarget(Entity<SmartFridgeComponent> ent, ref CanDropTargetEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        if (!CanInsertEntity(ent, args.User, args.Dragged))
        {
            args.CanDrop = false;
            return;
        }

        args.CanDrop = true;
    }

    private void OnDragDropTarget(Entity<SmartFridgeComponent> ent, ref DragDropTargetEvent args)
    {
        if (args.Handled)
            return;

        InsertEntity(ent, args.User, args.Dragged);
        args.Handled = true;
    }
    // Goobstation changes - Drag & Drop insert END
}
