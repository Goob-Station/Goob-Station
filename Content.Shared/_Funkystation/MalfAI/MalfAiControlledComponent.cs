// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared._Funkystation.MalfAI;

/// <summary>
/// Added to an entity (machine or borg) currently controlled by a Malf AI.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MalfAiControlledComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? Controller;

    [DataField, AutoNetworkedField]
    public string? UniqueId;
}
