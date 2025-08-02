// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Werewolf.Components;
using Content.Goobstation.Shared.Werewolf.Systems;
using Content.Goobstation.Shared.Werewolf.UI;
using Content.Server.Actions;

namespace Content.Goobstation.Server.Werewolf.Systems;

/// <summary>
/// This handles server-side logic for the mutation shop
/// </summary>
public sealed class WerewolfMutationShopSystem : SharedWerewolfMutationShopSystem
{
    [Dependency] private readonly SharedWerewolfTransformSystem _transformSystem = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WerewolfMutationShopComponent, ClaimedMessage>(OnClaimed);
    }

    private void OnClaimed(Entity<WerewolfMutationShopComponent> ent, ref ClaimedMessage args)
    {
        if (args.SelectedForm == null ||
            !TryComp<WerewolfTransformComponent>(ent.Owner, out var werewolfTransform))
            return;

        var transform = (ent.Owner, werewolfTransform);
        _transformSystem.ChangeForm(transform, args.SelectedForm.Value);

        // Remove the action since we have chosen a path to follow
        _actions.RemoveAction(ent.Comp.ActionEntity);
    }
}
