// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Content.Shared.Ninja.Components;

namespace Content.Shared.Ninja.Systems;

/// <summary>
/// Handles predicting that the action exists, creating items is done serverside.
/// </summary>
public abstract class SharedItemCreatorSystem : EntitySystem
{
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ItemCreatorComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ItemCreatorComponent, GetItemActionsEvent>(OnGetActions);
    }

    private void OnMapInit(Entity<ItemCreatorComponent> ent, ref MapInitEvent args)
    {
        var (uid, comp) = ent;
        // test funny dont mind me
        if (string.IsNullOrEmpty(comp.Action))
            return;

        _actionContainer.EnsureAction(uid, ref comp.ActionEntity, comp.Action);
        Dirty(uid, comp);
    }

    private void OnGetActions(Entity<ItemCreatorComponent> ent, ref GetItemActionsEvent args)
    {
        if (CheckItemCreator(ent, args.User))
            args.AddAction(ent.Comp.ActionEntity);
    }

    public bool CheckItemCreator(EntityUid uid, EntityUid user)
    {
        var ev = new CheckItemCreatorEvent(user);
        RaiseLocalEvent(uid, ref ev);
        return !ev.Cancelled;
    }
}

/// <summary>
/// Raised on the item creator before adding the action.
/// </summary>
[ByRefEvent]
public record struct CheckItemCreatorEvent(EntityUid User, bool Cancelled = false);

/// <summary>
/// Raised on the item creator before creating an item.
/// </summary>
[ByRefEvent]
public record struct CreateItemAttemptEvent(EntityUid User, bool Cancelled = false);