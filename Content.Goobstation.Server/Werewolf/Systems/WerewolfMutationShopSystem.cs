// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.Werewolf.Roles;
using Content.Goobstation.Shared.Werewolf.Components;
using Content.Goobstation.Shared.Werewolf.Systems;
using Content.Goobstation.Shared.Werewolf.UI;
using Content.Server.Actions;
using Content.Server.Mind;
using Content.Server.Roles;
using Content.Shared.Mind;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Werewolf.Systems;

/// <summary>
/// This handles server-side logic for the mutation shop
/// </summary>
public sealed class WerewolfMutationShopSystem : SharedWerewolfMutationShopSystem
{
    [Dependency] private readonly SharedWerewolfTransformSystem _transformSystem = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly TagSystem _tags = default!;
    [Dependency] private readonly RoleSystem _roles = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly MindSystem _mind = default!;
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

        // White Werewolf type shii
        if (!_proto.TryIndex(args.SelectedForm, out var index)
            || !index.Configuration.MakePeaceful
            || !_mind.TryGetMind(ent.Owner, out var mindId, out var mind))
            return;

        _roles.MindRemoveRole(mindId, "MindRoleWerewolf");
        _roles.MindAddRole(mindId, "MindRoleWhiteWerewolf", mind, silent: true);
    }
}
