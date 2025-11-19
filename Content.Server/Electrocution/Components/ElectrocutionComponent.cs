// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

﻿namespace Content.Server.Electrocution;

/// <summary>
/// Component for virtual electrocution entities (representing an in-progress shock).
/// </summary>
[RegisterComponent]
[Access(typeof(ElectrocutionSystem))]
public sealed partial class ElectrocutionComponent : Component
{
    [DataField("electrocuting")]
    public EntityUid Electrocuting;

    [DataField("source")]
    public EntityUid Source;

    [DataField("timeLeft")]
    public float TimeLeft;
}
