// SPDX-FileCopyrightText: 2023 avery <51971268+graevy@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT
using Content.Server.Shuttles.Systems;

namespace Content.Server.Shuttles.Components;

/// <summary>
/// Added to station emergency shuttles by <see cref="EmergencyShuttleSystem.AddEmergencyShuttle"/>,
/// for FTL event handlers
/// </summary>
[RegisterComponent]
public sealed partial class EmergencyShuttleComponent : Component
{

}