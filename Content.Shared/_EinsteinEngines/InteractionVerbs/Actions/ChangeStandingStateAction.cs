// SPDX-FileCopyrightText: 2024 Mnemotechnican <69920617+Mnemotechnician@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 RadsammyT <radsammyt@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Bed.Sleep;
using Content.Shared.InteractionVerbs;
using Content.Shared.Standing;

namespace Content.Server.InteractionVerbs.Actions;

[Serializable]
public sealed partial class ChangeStandingStateAction : InteractionAction
{
    [DataField]
    public bool MakeStanding, MakeLaying;

    public override bool CanPerform(InteractionArgs args, InteractionVerbPrototype proto, bool isBefore, VerbDependencies deps)
    {
        if (!deps.EntMan.TryGetComponent<StandingStateComponent>(args.Target, out var state))
            return false;

        if (isBefore)
            args.Blackboard["standing"] = state.Standing;

        return (state.Standing && MakeLaying)
               || (!state.Standing && MakeStanding);
    }

    public override bool Perform(InteractionArgs args, InteractionVerbPrototype proto, VerbDependencies deps)
    {
        var entMan = deps.EntMan;

        if (!entMan.TryGetComponent<StandingStateComponent>(args.Target, out var state))
            return false;

        if (args.TryGetBlackboard("standing", out bool oldStanding)
            && oldStanding != state.Standing)
            return false;

        var stateSystem = entMan.System<StandingStateSystem>();

        // Note: these will get cancelled if the target is forced to stand/lay, e.g. due to a buckle or stun or something else.
        if (!state.Standing && MakeStanding)
            return stateSystem.Stand(args.Target);

        if (state.Standing && MakeLaying)
            return stateSystem.Down(args.Target);

        return false;
    }
}
