// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Chat.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.MartialArts.Events;

[Serializable, NetSerializable, ImplicitDataDefinitionForInheritors]
public abstract partial class BaseCapoeiraEvent : EntityEventArgs
{
    [DataField]
    public virtual float VelocityPowerMultiplier { get; set; } = 0.7f;

    [DataField]
    public virtual float MinPower { get; set; } = 1f;

    [DataField]
    public virtual float MaxPower { get; set; } = 5f;

    [DataField]
    public virtual float MinVelocity { get; set; }

    [DataField]
    public virtual float AttackSpeedMultiplier { get; set; } = 1f;

    [DataField]
    public virtual TimeSpan AttackSpeedMultiplierTime { get; set; } = TimeSpan.Zero;

    [DataField]
    public virtual SoundSpecifier? Sound { get; set; } = new SoundPathSpecifier("/Audio/Weapons/genhit3.ogg");

    [DataField]
    public float StaminaToHeal = -20f;
}

public sealed partial class PushKickPerformedEvent : BaseCapoeiraEvent
{
    [DataField]
    public float ThrowRange = 1f;
}

public sealed partial class SweepKickPerformedEvent : BaseCapoeiraEvent;

public sealed partial class CircleKickPerformedEvent : BaseCapoeiraEvent
{
    [DataField]
    public TimeSpan SlowDownTime = TimeSpan.FromSeconds(2);
}

public sealed partial class SpinKickPerformedEvent : BaseCapoeiraEvent
{
    [DataField]
    public ProtoId<EmotePrototype>? Emote = "Flip";
}

public sealed partial class KickUpPerformedEvent : BaseCapoeiraEvent;
