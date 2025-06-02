// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;

namespace Content.Goobstation.Shared.Xenobiology;

public sealed partial class SlimeLatchEvent : EntityTargetActionEvent
{
    [DataField]
    public float Damage = 5;
}

public sealed partial class XenoVacEvent : EntityTargetActionEvent;

public sealed partial class XenoVacClearEvent : InstantActionEvent;
