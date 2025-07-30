// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Werewolf.Components;
using Content.Goobstation.Shared.Werewolf.Events;
using Content.Shared.Mind;
using Robust.Shared.Player;

namespace Content.Goobstation.Shared.Werewolf.Systems;

/// <summary>
/// This handles the mutation shop ability.
/// Opens the mutation shop
/// </summary>
public sealed class SharedWerewolfMutationShopSystem : EntitySystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _userInterface = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WerewolfMutationShopComponent, OpenMutationShopEvent>(OnOpenMutationShop);
    }

    private void OnOpenMutationShop(Entity<WerewolfMutationShopComponent> ent, ref OpenMutationShopEvent args)
    {
        TryOpenUi(ent.Owner);
    }

    private bool TryOpenUi(EntityUid target)
    {
        if (!_userInterface.HasUi(target, MutationUiKey.Key))
            return false;

        if (_mind.TryGetMind(target, out _, out var mindComp) &&
            _player.TryGetSessionById(mindComp.UserId, out var session) &&
            session is { } insession)
            _userInterface.OpenUi(target, MutationUiKey.Key, insession);

        return true;
    }
}
