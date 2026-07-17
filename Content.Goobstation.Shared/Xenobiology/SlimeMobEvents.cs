// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Xenobiology;

public sealed partial class SlimeLatchEvent : EntityTargetActionEvent
{
    [DataField]
    public float Damage = 5;
}

public sealed partial class EatCorpseEvent : EntityTargetActionEvent;

public sealed partial class XenoVacEvent : EntityTargetActionEvent;

public sealed partial class XenoVacClearEvent : InstantActionEvent;

[Serializable, NetSerializable]
public sealed partial class SlimeLatchDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class EatCorpseDoAfterEvent : SimpleDoAfterEvent;

/// <summary>
/// rised after mitosis completed, but before parent slime deletion, directed to parent slime
/// </summary>
public sealed partial class SlimeMitosisEvent : EntityEventArgs;
