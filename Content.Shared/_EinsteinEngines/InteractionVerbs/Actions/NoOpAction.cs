// SPDX-FileCopyrightText: 2024 Mnemotechnican <69920617+Mnemotechnician@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 RadsammyT <radsammyt@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.InteractionVerbs;
using Robust.Shared.Random;

namespace Content.Server.InteractionVerbs.Actions;

/// <summary>
///     An action that does nothing on its own, made just to mimic the old "chance to show a popup" interactions.
/// </summary>
[Serializable]
public sealed partial class NoOpAction : InteractionAction
{
    [DataField]
    public float SuccessChance = 1f;

    public override bool CanPerform(InteractionArgs args, InteractionVerbPrototype proto, bool isBefore, VerbDependencies deps)
    {
        if (isBefore)
            return true; // so the do-after can happen if there's one

        // Return true if chance >= 1f, false if <= 0f, or a random result if anywhere in-between.
        return SuccessChance > 0f && (SuccessChance >= 1f || deps.Random.Prob(SuccessChance));
    }

    public override bool Perform(InteractionArgs args, InteractionVerbPrototype proto, VerbDependencies deps)
    {
        return true;
    }
}
