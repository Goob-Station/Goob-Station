// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
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

public sealed partial class XenoVacEvent : EntityTargetActionEvent;

public sealed partial class XenoVacClearEvent : InstantActionEvent;

[Serializable, NetSerializable]
public sealed partial class SlimeLatchDoAfterEvent : SimpleDoAfterEvent;
