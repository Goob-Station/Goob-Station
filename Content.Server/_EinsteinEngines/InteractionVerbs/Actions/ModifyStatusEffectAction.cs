// SPDX-FileCopyrightText: 2024 Mnemotechnican <69920617+Mnemotechnician@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 RadsammyT <radsammyt@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.InteractionVerbs;
using Content.Shared.StatusEffect;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Server.InteractionVerbs.Actions;

[Serializable]
public sealed partial class ModifyStatusEffectAction : InteractionAction
{
    [DataField(required: true)]
    public ProtoId<StatusEffectPrototype> Effect;

    /// <summary>
    ///     If true, the action will ensure that the system already has the status effect when removing time,
    ///     or will ensure the entity gets the status effect when adding it.
    /// </summary>
    [DataField]
    public bool EnsureEffect = true;

    /// <summary>
    ///     Amount of time added by this action. Can be negative, but then <see cref="EnsureEffect"/> should be false.
    /// </summary>
    [DataField]
    public TimeSpan TimeAdded = TimeSpan.FromSeconds(1);

    public override bool CanPerform(InteractionArgs args, InteractionVerbPrototype proto, bool isBefore, VerbDependencies deps)
    {
        var statusEffects = deps.EntMan.System<StatusEffectsSystem>();
        if (!statusEffects.CanApplyEffect(args.Target, Effect))
            return false;

        return !EnsureEffect || TimeAdded >= TimeSpan.Zero || statusEffects.HasStatusEffect(args.Target, Effect);
    }

    public override bool Perform(InteractionArgs args, InteractionVerbPrototype proto, VerbDependencies deps)
    {
        var statusEffects = deps.EntMan.System<StatusEffectsSystem>();

        if (statusEffects.HasStatusEffect(args.Target, Effect))
            return statusEffects.TryAddTime(args.Target, Effect, TimeAdded);
        else if (EnsureEffect)
            return statusEffects.TryAddStatusEffect(args.Target, Effect, TimeAdded, true);

        return false;
    }
}
