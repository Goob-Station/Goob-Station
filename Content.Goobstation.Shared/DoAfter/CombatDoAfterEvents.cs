// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Robust.Shared.Audio;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.DoAfter;

[Serializable, NetSerializable]
public sealed partial class CombatDoAfterEvent : SimpleDoAfterEvent;

[ImplicitDataDefinitionForInheritors]
public abstract partial class BaseCombatDoAfterSuccessEvent : EntityEventArgs;

[Virtual]
public partial class CombatDoAfterMeleeHitEvent : BaseCombatDoAfterSuccessEvent
{
    public IReadOnlyList<EntityUid> Targets;

    public DamageSpecifier BonusDamage = new();
}

[Virtual]
public partial class CombatDoAfterThrownEvent : BaseCombatDoAfterSuccessEvent;

public sealed partial class CombatSyringeTriggerEvent : CombatDoAfterMeleeHitEvent
{
    [DataField]
    public SoundSpecifier? InjectSound = new SoundPathSpecifier("/Audio/_Goobstation/Weapons/Effects/pierce1.ogg");

    [DataField]
    public float SolutionSplitFraction = 1f;

    [DataField]
    public DamageSpecifier SyringeExtraDamage = new()
    {
        DamageDict =
        {
            { "Piercing", 4 },
        },
    };
}
