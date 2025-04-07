// SPDX-FileCopyrightText: 2022 Kara D <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tayrtahn <tayrtahn@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Animations;
using Robust.Shared.GameStates;

namespace Content.Shared.Follower.Components;

[RegisterComponent]
[NetworkedComponent]
public sealed partial class OrbitVisualsComponent : Component
{
    /// <summary>
    ///     How long should the orbit animation last in seconds, before being randomized?
    /// </summary>
    public float OrbitLength = 2.0f;

    /// <summary>
    ///     How far away from the entity should the orbit be, before being randomized?
    /// </summary>
    public float OrbitDistance = 1.0f;

    /// <summary>
    ///     How long should the orbit stop animation last in seconds?
    /// </summary>
    public float OrbitStopLength = 1.0f;
}