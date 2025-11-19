// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Content.Shared.Cloning;
using Robust.Shared.Prototypes;

namespace Content.Server.Cloning.Components;

/// <summary>
///     This is added to a marker entity in order to spawn a clone of a random player.
/// </summary>
[RegisterComponent, EntityCategory("Spawner")]
public sealed partial class RandomCloneSpawnerComponent : Component
{
    /// <summary>
    ///     Cloning settings to be used.
    /// </summary>
    [DataField]
    public ProtoId<CloningSettingsPrototype> Settings = "BaseClone";
}
