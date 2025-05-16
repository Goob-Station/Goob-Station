// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;

namespace Content.Goobstation.Shared.PairedExtendable;

[RegisterComponent]
public sealed partial class LeftPairedExtendableUserComponent : PairedExtendableUserComponent
{
    [DataField]
    public string ActionProto = "ActionToggleLeftMantisBlade";
}

public sealed partial class ToggleLeftExtendableEvent : InstantActionEvent;
