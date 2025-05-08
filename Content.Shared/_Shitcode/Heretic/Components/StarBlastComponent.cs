// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Shitcode.Heretic.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause]
public sealed partial class StarBlastComponent : Component
{
    [DataField]
    public float CosmicFieldRadius = 0.5f;

    [DataField]
    public float CosmicFieldLifetime = 5f;

    [DataField]
    public float StarMarkRadius = 3f;

    [DataField]
    public TimeSpan StarMarkDuration = TimeSpan.FromSeconds(30);

    [DataField, AutoPausedField]
    public TimeSpan NextCosmicFieldTime = TimeSpan.Zero;

    [DataField]
    public TimeSpan CosmicFieldPeriod = TimeSpan.FromSeconds(0.25f);

    [DataField]
    public TimeSpan KnockdownTime = TimeSpan.FromSeconds(4);
}
