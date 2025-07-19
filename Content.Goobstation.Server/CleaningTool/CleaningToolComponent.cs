// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.CleaningTool;

/// <summary>
/// This component is for items that can clean stuff like footprints, stains, etcetera.
/// Cleaning != Cleaning forensics.
/// </summary>
[RegisterComponent]
public sealed partial class CleaningToolComponent : Component
{
    /// <summary>
    /// How long it takes to destroy footprints, strain, etcetera off of things using this entity
    /// </summary>
    [DataField]
    public float CleanDelay = 8.0f;

    /// <summary>
    /// The X by X box this utensil will clean in.
    /// This is one for bars of soap, three for mops.
    /// </summary>
    [DataField]
    public int Radius = 1;

    /// <summary>
    /// The prototype for the sparkle effect to spawn.
    /// </summary>
    [DataField]
    public EntProtoId SparkleProto = "PuddleSparkle";
}
