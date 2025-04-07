// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
namespace Content.Server._Lavaland.Tendril.Components;

/// <summary>
/// A mob created by a tendril. Upon death, it is removed from its spawn list
/// </summary>
[RegisterComponent]
public sealed partial class TendrilMobComponent : Component
{
    public EntityUid? Tendril;
}