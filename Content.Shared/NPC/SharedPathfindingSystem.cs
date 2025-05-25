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

using System.Numerics;

namespace Content.Shared.NPC;

public abstract partial class SharedPathfindingSystem : EntitySystem
{
    /// <summary>
    /// This is equivalent to agent radii for navmeshes. In our case it's preferable that things are cleanly
    /// divisible per tile so we'll make sure it works as a discrete number.
    /// </summary>
    public const byte SubStep = 4;

    public const byte ChunkSize = 8;
    public static readonly Vector2 ChunkSizeVec = new(ChunkSize, ChunkSize);

    /// <summary>
    /// We won't do points on edges so we'll offset them slightly.
    /// </summary>
    protected const float StepOffset = 1f / SubStep / 2f;

    private static readonly Vector2 StepOffsetVec = new(StepOffset, StepOffset);

    public Vector2 GetCoordinate(Vector2i chunk, Vector2i index)
    {
        return new Vector2(index.X, index.Y) / SubStep+ (chunk) * ChunkSizeVec + StepOffsetVec;
    }

    public static float ManhattanDistance(Vector2i start, Vector2i end)
    {
        var distance = end - start;
        return Math.Abs(distance.X) + Math.Abs(distance.Y);
    }

    public static float OctileDistance(Vector2i start, Vector2i end)
    {
        var diff = start - end;
        var ab = Vector2.Abs(diff);
        return ab.X + ab.Y + (1.41f - 2) * Math.Min(ab.X, ab.Y);
    }

    public static IEnumerable<Vector2i> GetTileOutline(Vector2i center, float radius)
    {
        // https://www.redblobgames.com/grids/circle-drawing/
        var vecCircle = center + Vector2.One / 2f;

        for (var r = 0; r <= Math.Floor(radius * MathF.Sqrt(0.5f)); r++)
        {
            var d = MathF.Floor(MathF.Sqrt(radius * radius - r * r));

            yield return new Vector2(vecCircle.X - d, vecCircle.Y + r).Floored();

            yield return new Vector2(vecCircle.X + d, vecCircle.Y + r).Floored();

            yield return new Vector2(vecCircle.X - d, vecCircle.Y - r).Floored();

            yield return new Vector2(vecCircle.X + d, vecCircle.Y - r).Floored();

            yield return new Vector2(vecCircle.X + r, vecCircle.Y - d).Floored();

            yield return new Vector2(vecCircle.X + r, vecCircle.Y + d).Floored();

            yield return new Vector2(vecCircle.X - r, vecCircle.Y - d).Floored();

            yield return new Vector2(vecCircle.X - r, vecCircle.Y + d).Floored();
        }
    }
}