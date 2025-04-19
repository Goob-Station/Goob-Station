// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.MartialArts.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class DragonKungFuTimerComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan LastMoveTime = TimeSpan.Zero;

    [DataField]
    public float MinVelocitySquared = 0.25f;

    [DataField]
    public TimeSpan PauseDuration = TimeSpan.FromSeconds(1f);

    [DataField]
    public TimeSpan BuffLength = TimeSpan.FromSeconds(5f);
}
