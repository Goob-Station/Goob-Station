// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 I.K <45953835+notquitehadouken@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 notquitehadouken <tripwiregamer@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;

namespace Content.Shared.Storage;

[DataDefinition, Serializable, NetSerializable]
public partial record struct ItemStorageLocation
{
    /// <summary>
    /// The rotation, stored a cardinal direction in order to reduce rounding errors.
    /// </summary>
    [DataField("_rotation")]
    public Direction Direction;

    /// <summary>
    /// The rotation of the piece in storage.
    /// </summary>
    public Angle Rotation
    {
        get => Direction.ToAngle();
        set => Direction = value.GetCardinalDir();
    }

    /// <summary>
    /// Where the item is located in storage.
    /// </summary>
    [DataField]
    public Vector2i Position;

    public ItemStorageLocation(Angle rotation, Vector2i position)
    {
        Rotation = rotation;
        Position = position;
    }

    public bool Equals(ItemStorageLocation? other)
    {
        return Rotation == other?.Rotation &&
               Position == other.Value.Position;
    }
};