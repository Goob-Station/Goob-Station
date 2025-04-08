// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Shuttles.Systems;

namespace Content.Server.Shuttles.Components;

/// <summary>
/// Prevents FTL from occuring around this entity.
/// </summary>
[RegisterComponent, Access(typeof(SharedShuttleSystem))]
public sealed partial class FTLExclusionComponent : Component
{
    [DataField]
    public bool Enabled = true;

    [DataField(required: true)]
    public float Range = 32f;
}