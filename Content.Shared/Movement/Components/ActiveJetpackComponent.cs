// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 rolfero <45628623+rolfero@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT
using Robust.Shared.GameStates;

namespace Content.Shared.Movement.Components;

/// <summary>
/// Added to an enabled jetpack. Tracks gas usage on server / effect spawning on client.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ActiveJetpackComponent : Component
{
    public float EffectCooldown = 0.3f;
    public TimeSpan TargetTime = TimeSpan.Zero;
}