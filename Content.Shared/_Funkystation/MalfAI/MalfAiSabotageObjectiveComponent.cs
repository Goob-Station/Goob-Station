// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

namespace Content.Shared._Funkystation.MalfAI;

public enum MalfAiSabotageType : byte
{
    Doomsday,
    Assassinate,
    Protect,
}

/// <summary>
/// Tracks which sabotage type this objective requires.
/// </summary>
[RegisterComponent]
public sealed partial class MalfAiSabotageObjectiveComponent : Component
{
    [DataField(required: true)]
    public MalfAiSabotageType SabotageType;
}
