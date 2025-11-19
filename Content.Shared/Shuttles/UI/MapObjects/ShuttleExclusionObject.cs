// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared.Shuttles.UI.MapObjects;

[Serializable, NetSerializable]
public record struct ShuttleExclusionObject(NetCoordinates Coordinates, float Range, string Name = "") : IMapObject
{
    public bool HideButton => false;
}
