// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

﻿namespace Content.Server.RequiresGrid;

/// <summary>
/// Destroys an entity when they no longer are part of a Grid
/// </summary>
[RegisterComponent]
[Access(typeof(RequiresGridSystem))]
public sealed partial class RequiresGridComponent : Component
{

}
