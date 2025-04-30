// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 LordCarve <27449516+LordCarve@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Ame.EntitySystems;
using Content.Shared.Ame.Components;

namespace Content.Server.Ame.Components;

/// <summary>
/// The component used to make an entity part of the bulk machinery of an AntiMatter Engine.
/// Connects to adjacent entities with this component or <see cref="AmeControllerComponent"/> to make an AME.
/// </summary>
[Access(typeof(AmeShieldingSystem), typeof(AmeNodeGroup))]
[RegisterComponent]
public sealed partial class AmeShieldComponent : SharedAmeShieldComponent
{
    /// <summary>
    /// Whether or not this AME shield counts as a core for the AME or not.
    /// </summary>
    [ViewVariables]
    public bool IsCore = false;

    /// <summary>
    /// The current integrity of the AME shield.
    /// </summary>
    [DataField("integrity")]
    [ViewVariables]
    public int CoreIntegrity = 100;
}