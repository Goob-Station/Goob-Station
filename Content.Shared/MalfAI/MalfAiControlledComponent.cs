// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;

namespace Content.Shared.MalfAI;

/// <summary>
/// Marks a silicon (borg) as having been law-imposed by a specific Malf AI.
/// Stores a controller reference and a unique identifier useful for targeting/display.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MalfAiControlledComponent : Component
{
    /// <summary>
    /// The Malfunctioning AI entity that imposed Law 0 on this borg.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Controller;

    /// <summary>
    /// A unique string identifier assigned when control is first established.
    /// Primarily for easier referencing/targeting in gameplay or debugging.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? UniqueId;
}
