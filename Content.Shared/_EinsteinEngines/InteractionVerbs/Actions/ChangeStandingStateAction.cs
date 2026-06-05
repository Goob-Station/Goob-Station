// SPDX-FileCopyrightText: 2024 Mnemotechnican <69920617+Mnemotechnician@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 RadsammyT <radsammyt@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Bed.Sleep;
using Content.Shared.InteractionVerbs;
using Content.Shared.Standing;
using Content.Shared.Stunnable;

namespace Content.Shared._EinsteinEngines.InteractionVerbs.Actions;

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
        var stun = entMan.System<SharedStunSystem>();
        // Note: these will get cancelled if the target is forced to stand/lay, e.g. due to a buckle or stun or something else.
        if (!state.Standing && MakeStanding &&
            entMan.TryGetComponent<KnockedDownComponent>(args.Target, out var knocked))
        {
            stun.ForceStandUp((args.Target, knocked));
            return state.Standing;
        }

        if (state.Standing && MakeLaying)
            return stun.TryCrawling(args.Target, autoStand: false);

        return false;
    }
}
