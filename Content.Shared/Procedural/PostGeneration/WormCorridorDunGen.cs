// SPDX-FileCopyrightText: 2024 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.Procedural.PostGeneration;

// Ime a worm
/// <summary>
/// Generates worm corridors.
/// </summary>
public sealed partial class WormCorridorDunGen : IDunGenLayer
{
    [DataField]
    public int PathLimit = 2048;

    /// <summary>
    /// How many times to run the worm
    /// </summary>
    [DataField]
    public int Count = 20;

    /// <summary>
    /// How long to make each worm
    /// </summary>
    [DataField]
    public int Length = 20;

    /// <summary>
    /// Maximum amount the angle can change in a single step.
    /// </summary>
    [DataField]
    public Angle MaxAngleChange = Angle.FromDegrees(45);

    /// <summary>
    /// How wide to make the corridor.
    /// </summary>
    [DataField]
    public float Width = 3f;
}