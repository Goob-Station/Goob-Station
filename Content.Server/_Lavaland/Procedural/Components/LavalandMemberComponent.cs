// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
namespace Content.Server._Lavaland.Procedural.Components;

/// <summary>
/// Component that is used for displaying GPS locator points on it's UI.
/// </summary>
[RegisterComponent]
public sealed partial class LavalandMemberComponent : Component
{
    /// <summary>
    /// Name that is going to be displayed at GPS.
    /// </summary>
    [DataField]
    public LocId SignalName;
}