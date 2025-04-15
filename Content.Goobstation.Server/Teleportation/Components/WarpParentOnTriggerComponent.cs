// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Rouge2t7 <sarahoneill132@hotmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Server.Teleportation.Components;

[RegisterComponent]
public sealed partial class WarpParentOnTriggerComponent : Component
{
    [DataField] public string WarpLocation { get; set; } = "CentComm";
}
