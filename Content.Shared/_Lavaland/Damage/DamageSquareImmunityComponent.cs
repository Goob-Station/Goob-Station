// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
namespace Content.Shared._Lavaland.Damage;

/// <summary>
/// Actor having this component will not get damaged by damage squares.
/// </summary>
[RegisterComponent]
public sealed partial class DamageSquareImmunityComponent : Component
{
    [DataField]
    public TimeSpan HasImmunityUntil = TimeSpan.Zero;

    /// <summary>
    /// Setting this to true will ignore the timer and will make damage tile completely ignore an entity.
    /// </summary>
    [DataField]
    public bool IsImmune;
}