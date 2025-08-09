// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Shared.Procedural.DungeonGenerators;

/// <summary>
/// Runs another <see cref="DungeonConfig"/>.
/// Used for storing data on 1 system.
/// </summary>
public sealed partial class PrototypeDunGen : IDunGenLayer
{
    /// <summary>
    /// Should we pass in the current level's dungeons to the prototype.
    /// </summary>
    [DataField]
    public DungeonInheritance InheritDungeons = DungeonInheritance.None;

    [DataField(required: true)]
    public ProtoId<DungeonConfigPrototype> Proto;
}

public enum DungeonInheritance : byte
{
    /// <summary>
    /// Don't inherit any of the current layer's dungeons for this <see cref="PrototypeDunGen"/>
    /// </summary>
    None,

    /// <summary>
    /// Inherit only the last dungeon ran.
    /// </summary>
    Last,

    /// <summary>
    /// Inherit all of the current layer's dungeons.
    /// </summary>
    All,
}
