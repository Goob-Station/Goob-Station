// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server.NPC.Components;

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class NPCJukeComponent : Component
{
    [DataField("jukeType")]
    public JukeType JukeType = JukeType.Away;

    [DataField("jukeDuration")]
    public float JukeDuration = 0.5f;

    [DataField("nextJuke", customTypeSerializer:typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan NextJuke;

    [DataField("targetTile")]
    public Vector2i? TargetTile;
}

public enum JukeType : byte
{
    /// <summary>
    /// Will move directly away from target if applicable.
    /// </summary>
    Away,

    /// <summary>
    /// Move to the adjacent tile for the specified duration.
    /// </summary>
    AdjacentTile
}