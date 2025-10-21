// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.MartialArts.Events;

[Serializable, NetSerializable, ImplicitDataDefinitionForInheritors]
public abstract partial class BaseKungFuDragonEvent : EntityEventArgs
{
    [DataField]
    public virtual SoundSpecifier Sound { get; set; } = new SoundPathSpecifier("/Audio/Weapons/genhit3.ogg");
}

[DataDefinition]
public sealed partial class DragonClawPerformedEvent : BaseKungFuDragonEvent
{
    [DataField]
    public TimeSpan SpeedUpTime = TimeSpan.FromSeconds(2);

    [DataField]
    public float SpeedMultiplier = 1.3f;
}

[DataDefinition]
public sealed partial class DragonTailPerformedEvent : BaseKungFuDragonEvent
{
    [DataField]
    public TimeSpan DownedParalyzeTime = TimeSpan.FromSeconds(1);
}

[DataDefinition]
public sealed partial class DragonStrikePerformedEvent : BaseKungFuDragonEvent;
