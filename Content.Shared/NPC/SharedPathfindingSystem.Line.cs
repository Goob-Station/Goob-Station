// SPDX-FileCopyrightText: 2024 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Errant <35878406+Errant-4@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 IProduceWidgets <107586145+IProduceWidgets@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 JustCone <141039037+JustCone14@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Mervill <mervills.email@gmail.com>
// SPDX-FileCopyrightText: 2024 PJBot <pieterjan.briers+bot@gmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 PopGamer46 <yt1popgamer@gmail.com>
// SPDX-FileCopyrightText: 2024 Spessmann <156740760+Spessmann@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Winkarst <74284083+Winkarst-cpu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 coolboy911 <85909253+coolboy911@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 lunarcomets <140772713+lunarcomets@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 saintmuntzer <47153094+saintmuntzer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.NPC;

public abstract partial class SharedPathfindingSystem
{
    public static void GridCast(Vector2i start, Vector2i end, Vector2iCallback callback)
    {
        // https://gist.github.com/Pyr3z/46884d67641094d6cf353358566db566
        // declare all locals at the top so it's obvious how big the footprint is
        int dx, dy, xinc, yinc, side, i, error;

        // starting cell is always returned
        if (!callback(start))
            return;

        xinc  = (end.X < start.X) ? -1 : 1;
        yinc  = (end.Y < start.Y) ? -1 : 1;
        dx    = xinc * (end.X - start.X);
        dy    = yinc * (end.Y - start.Y);
        var ax = start.X;
        var ay = start.Y;

        if (dx == dy) // Handle perfect diagonals
        {
            // I include this "optimization" for more aesthetic reasons, actually.
            // While Bresenham's Line can handle perfect diagonals just fine, it adds
            // additional cells to the line that make it not a perfect diagonal
            // anymore. So, while this branch is ~twice as fast as the next branch,
            // the real reason it is here is for style.

            // Also, there *is* the reason of performance. If used for cell-based
            // raycasts, for example, then perfect diagonals will check half as many
            // cells.

            while (dx --> 0)
            {
                ax += xinc;
                ay += yinc;
                if (!callback(new Vector2i(ax, ay)))
                    return;
            }

            return;
        }

        // Handle all other lines

        side = -1 * ((dx == 0 ? yinc : xinc) - 1);

        i     = dx + dy;
        error = dx - dy;

        dx *= 2;
        dy *= 2;

        while (i --> 0)
        {
            if (error > 0 || error == side)
            {
                ax    += xinc;
                error -= dy;
            }
            else
            {
                ay    += yinc;
                error += dx;
            }

            if (!callback(new Vector2i(ax, ay)))
                return;
        }
    }

    public delegate bool Vector2iCallback(Vector2i index);
}