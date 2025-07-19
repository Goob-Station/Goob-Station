// SPDX-FileCopyrightText: 2024 Mnemotechnican <69920617+Mnemotechnician@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 RadsammyT <radsammyt@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Content.Shared.InteractionVerbs;
using Robust.Shared.Serialization;

namespace Content.Server.InteractionVerbs.Actions;

[Serializable]
public sealed partial class ModifyHealthAction : InteractionAction
{
    [DataField(required: true)] public DamageSpecifier Damage = default!;
    [DataField] public bool IgnoreResistance = false;

    public override bool IsAllowed(InteractionArgs args, InteractionVerbPrototype proto, VerbDependencies deps)
    {
        return deps.EntMan.HasComponent<DamageableComponent>(args.Target);
    }

    public override bool CanPerform(InteractionArgs args, InteractionVerbPrototype proto, bool beforeDelay, VerbDependencies deps)
    {
        // TODO: check if container supports this kind of damage?
        return true;
    }

    public override bool Perform(InteractionArgs args, InteractionVerbPrototype proto, VerbDependencies deps)
    {
        return deps.EntMan.System<DamageableSystem>()
            .TryChangeDamage(args.Target, Damage, IgnoreResistance, origin: args.User) is not null;
    }
}
