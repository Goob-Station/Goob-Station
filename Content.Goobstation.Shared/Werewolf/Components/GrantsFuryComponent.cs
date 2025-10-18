// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Shared.Werewolf.Components;

[RegisterComponent]
public sealed partial class GrantsFuryComponent : Component
{
    /// <summary>
    /// The amount of fury the specified organ (mostly) gives.
    /// </summary>
    /// <returns></returns>
    [DataField]
    public int Fury = 2;
}

/// <summary>
/// Raised directly to the entity that ate an organ/food marked with the GrantsFury component
/// </summary>
/// <param name="FuryToGrant"></param> The fury to grant to the eater of said organ/food
[ByRefEvent]
public record struct EatenFuryEvent(int FuryToGrant);
