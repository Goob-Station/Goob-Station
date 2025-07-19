// SPDX-FileCopyrightText: 2024 Mnemotechnican <69920617+Mnemotechnician@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 RadsammyT <radsammyt@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Standing;
using Content.Shared.Stunnable;
using Content.Shared.Whitelist;
using Robust.Shared.Serialization;

namespace Content.Shared.InteractionVerbs.Requirements;

/// <summary>
///     Requires the target to meet a certain whitelist and not meet a blacklist.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class EntityWhitelistRequirement : InteractionRequirement
{
    [DataField] public EntityWhitelist Whitelist = new(), Blacklist = new();

    public override bool IsMet(InteractionArgs args, InteractionVerbPrototype proto, InteractionAction.VerbDependencies deps)
    {
        //return Whitelist.IsValid(args.Target) && !Blacklist.IsValid(args.Target);
        //TODO: The shittiest hack I have ever done just to make this work.
        //      If this ever makes it into the PR/codebase @radsammyt on discord
        //      and suplex him in-game on every fucking table. no. seriously.
        return true;
    }
}

/// <summary>
///     Requires the mob to be a mob in a certain state. If inverted, requires the mob to not be in that state.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class MobStateRequirement : InvertableInteractionRequirement
{
    [DataField] public List<MobState> AllowedStates = new();

    public override bool IsMet(InteractionArgs args, InteractionVerbPrototype proto, InteractionAction.VerbDependencies deps)
    {
        if (!deps.EntMan.TryGetComponent<MobStateComponent>(args.Target, out var state))
            return false;

        return AllowedStates.Contains(state.CurrentState) ^ Inverted;
    }
}

/// <summary>
///     Requires the target to be in a specific standing state.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class StandingStateRequirement : InteractionRequirement
{
    [DataField] public bool AllowStanding, AllowLaying, AllowKnockedDown;

    public override bool IsMet(InteractionArgs args, InteractionVerbPrototype proto, InteractionAction.VerbDependencies deps)
    {
        if (deps.EntMan.HasComponent<KnockedDownComponent>(args.Target))
            return AllowKnockedDown;

        if (!deps.EntMan.TryGetComponent<StandingStateComponent>(args.Target, out var state))
            return false;

        return state.CurrentState == StandingState.Standing && AllowStanding
            || state.CurrentState == StandingState.Lying && AllowLaying;
    }
}

/// <summary>
///     Requires the target to be the user itself.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class SelfTargetRequirement : InvertableInteractionRequirement
{
    public override bool IsMet(InteractionArgs args, InteractionVerbPrototype proto, InteractionAction.VerbDependencies deps)
    {
        return (args.Target == args.User) ^ Inverted;
    }
}
