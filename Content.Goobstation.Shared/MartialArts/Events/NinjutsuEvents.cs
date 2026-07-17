// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.MartialArts.Events;

[Serializable, NetSerializable, ImplicitDataDefinitionForInheritors]
public abstract partial class BaseNinjutsuEvent : EntityEventArgs
{
    [DataField]
    public virtual SoundSpecifier Sound { get; set; } = new SoundPathSpecifier("/Audio/Weapons/genhit3.ogg");
}

public sealed partial class NinjutsuTakedownPerformedEvent : BaseNinjutsuEvent
{
    [DataField]
    public float BackstabMultiplier = 2.5f;
}

public sealed partial class BiteTheDustPerformedEvent : BaseNinjutsuEvent;

public sealed partial class DirtyKillPerformedEvent : BaseNinjutsuEvent;
