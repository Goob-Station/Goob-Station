// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.MartialArts.Events;

[Serializable, NetSerializable, DataDefinition]
public sealed partial class JudoDiscombobulatePerformedEvent : EntityEventArgs
{
    [DataField]
    public TimeSpan Time = TimeSpan.FromSeconds(10);

    [DataField]
    public float SpeedMultiplier = 0.7f;

    [DataField]
    public float StaminaResistanceModifier = 1.2f;

    [DataField]
    public EntProtoId StatusEffectProto = "StatusEffectStaminaResistanceModifier";
}

[Serializable, NetSerializable, DataDefinition]
public sealed partial class JudoEyePokePerformedEvent : EntityEventArgs;

[Serializable, NetSerializable, DataDefinition]
public sealed partial class JudoThrowPerformedEvent : EntityEventArgs;

[Serializable, NetSerializable, DataDefinition]
public sealed partial class JudoArmbarPerformedEvent : EntityEventArgs;

[Serializable, NetSerializable, DataDefinition]
public sealed partial class JudoWheelThrowPerformedEvent : EntityEventArgs;

[Serializable, NetSerializable, DataDefinition]
public sealed partial class JudoGoldenBlastPerformedEvent : EntityEventArgs;
