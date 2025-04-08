// SPDX-FileCopyrightText: 2023 Morb <14136326+Morb0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Objectives.Systems;

namespace Content.Server.Objectives.Components;

/// <summary>
/// Objective condition that requires the player to leave station of escape shuttle with only antags on board or handcuffed humanoids
/// </summary>
[RegisterComponent, Access(typeof(HijackShuttleConditionSystem))]
public sealed partial class HijackShuttleConditionComponent : Component
{
}