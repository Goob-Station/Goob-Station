// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Robust.Client.Placement;
using Robust.Shared.Map;

namespace Content.Client.Placement.Modes
{
    public sealed class WallmountLight : PlacementMode
    {
        public WallmountLight(PlacementManager pMan) : base(pMan)
        {
        }

        public override void AlignPlacementMode(ScreenCoordinates mouseScreen)
        {
            MouseCoords = ScreenToCursorGrid(mouseScreen);
            CurrentTile = GetTileRef(MouseCoords);

            if (pManager.CurrentPermission!.IsTile)
            {
                return;
            }

            var tileCoordinates = new EntityCoordinates(MouseCoords.EntityId, CurrentTile.GridIndices);

            Vector2 offset;
            switch (pManager.Direction)
            {
                case Direction.North:
                    offset = new Vector2(0.5f, 1f);
                    break;
                case Direction.South:
                    offset = new Vector2(0.5f, 0f);
                    break;
                case Direction.East:
                    offset = new Vector2(1f, 0.5f);
                    break;
                case Direction.West:
                    offset = new Vector2(0f, 0.5f);
                    break;
                default:
                    return;
            }

            tileCoordinates = tileCoordinates.Offset(offset);
            MouseCoords = tileCoordinates;
        }

        public override bool IsValidPosition(EntityCoordinates position)
        {
            if (pManager.CurrentPermission!.IsTile)
            {
                return false;
            }
            else if (!RangeCheck(position))
            {
                return false;
            }

            return true;
        }
    }
}