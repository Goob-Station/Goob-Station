// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goidastation.Shared.Stacks;
using Content.Server.Stack;
using Content.Shared.Stacks;

namespace Content.Goidastation.Server.Stacks;

/// <summary>
/// This handles...
/// </summary>
public sealed class GoidaStackSystem : GoidaSharedStackSystem
{

    [Dependency] private readonly StackSystem _stackSystem = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
    }

    // Goidastation - Custom stack splitting dialog
    protected override void OnCustomSplitMessage(Entity<StackComponent> ent, ref StackCustomSplitAmountMessage message)
    {
        var (uid, comp) = ent;

        // digital ghosts shouldn't be allowed to split stacks
        if (!(message.Actor is { Valid: true } user))
            return;

        var amount = message.Amount;
        _stackSystem.UserSplit(uid, user, amount, comp);
    }
}