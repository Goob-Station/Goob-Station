// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Server.StationEvents.Components;

/// <summary>
///   Adjusts chaos of the game director over time
/// </summary>
[RegisterComponent]
public sealed partial class ChaosAdjusterComponent : Component
{
    /// <summary>
    ///   By how much to adjust chaos per second
    /// </summary>
    [DataField(required: true)]
    public float Amount = 0f; // for reference, at the time this was written, -0.005/s is the amount each living person adjusts chaos by
}
