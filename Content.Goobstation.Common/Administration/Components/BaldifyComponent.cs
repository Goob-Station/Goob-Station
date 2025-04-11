// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Goobstation.Common.Administration.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class BaldifyComponent : Component
{
    [DataField]
    public string TargetLayer = "human_hair.rsi";

    public int? TargetIndex;
}