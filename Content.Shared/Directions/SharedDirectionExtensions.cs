// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Map;

namespace Content.Shared.Directions;

public static class SharedDirectionExtensions
{
    public static EntityCoordinates Offset(this EntityCoordinates coordinates, Direction direction)
    {
        return coordinates.Offset(direction.ToVec());
    }
}