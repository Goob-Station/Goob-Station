// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Security.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared.Security.Components;

[RegisterComponent, NetworkedComponent]
[Access(typeof(DeployableBarrierSystem))]
public sealed partial class DeployableBarrierComponent : Component
{
    /// <summary>
    ///     The fixture to change collision on.
    /// </summary>
    [DataField("fixture", required: true)] public string FixtureId = string.Empty;
}