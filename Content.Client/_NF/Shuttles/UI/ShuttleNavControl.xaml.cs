// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

// New Frontiers - This file is licensed under AGPLv3
// Copyright (c) 2024 New Frontiers Contributors
// See AGPLv3.txt for details.

using Content.Shared._NF.Shuttles.Events;
using Content.Shared.Shuttles.BUIStates;
using System.Numerics;
using Content.Shared.Shuttles.Components;
using Robust.Client.Graphics;
using Robust.Shared.Collections;

// Purposefully colliding with base namespace.
namespace Content.Client.Shuttles.UI;

public sealed partial class ShuttleNavControl
{
    public InertiaDampeningMode DampeningMode { get; set; }

    /// <summary>
    /// Whether the shuttle is currently in FTL. This is used to disable the Park button
    /// while in FTL to prevent parking while traveling.
    /// </summary>
    public bool InFtl { get; set; }

    private void NfUpdateState(NavInterfaceState state)
    {
        if (!EntManager.GetCoordinates(state.Coordinates).HasValue ||
            !EntManager.TryGetComponent(EntManager.GetCoordinates(state.Coordinates).GetValueOrDefault().EntityId, out TransformComponent? transform))
            return;

        DampeningMode = state.DampeningMode;

        // Check if the entity has an FTLComponent which indicates it's in FTL
        InFtl = transform.GridUid != null && EntManager.HasComponent<FTLComponent>(transform.GridUid);
    }

    // New Frontiers - Maximum IFF Distance - checks distance to object, draws if closer than max range
    // This code is licensed under AGPLv3. See AGPLv3.txt
    private bool NfCheckShouldDrawIffRangeCondition(bool shouldDrawIff, Vector2 distance)
    {
        if (shouldDrawIff && MaximumIFFDistance >= 0.0f)
        {
            if (distance.Length() > MaximumIFFDistance)
            {
                shouldDrawIff = false;
            }
        }

        return shouldDrawIff;
    }

    private static void NfAddBlipToList(List<BlipData> blipDataList,
        bool isOutsideRadarCircle,
        Vector2 uiPosition,
        int uiXCentre,
        int uiYCentre,
        Color color)
    {
        blipDataList.Add(new BlipData
        {
            IsOutsideRadarCircle = isOutsideRadarCircle,
            UiPosition = uiPosition,
            VectorToPosition = uiPosition - new Vector2(uiXCentre, uiYCentre),
            Color = color
        });
    }

    /**
    * Frontier - Adds blip style triangles that are on ships or pointing towards ships on the edges of the radar.
    * Draws blips at the BlipData's uiPosition and uses VectorToPosition to rotate to point towards ships.
    */
    private void NfDrawBlips(DrawingHandleBase handle, List<BlipData> blipDataList)
    {
        var blipValueList = new Dictionary<Color, ValueList<Vector2>>();

        foreach (var blipData in blipDataList)
        {
            var triangleShapeVectorPoints = new[]
            {
                new Vector2(0, 0),
                new Vector2(RadarBlipSize, 0),
                new Vector2(RadarBlipSize * 0.5f, RadarBlipSize)
            };

            if (blipData.IsOutsideRadarCircle)
            {
                // Calculate the angle of rotation
                var angle = (float) Math.Atan2(blipData.VectorToPosition.Y, blipData.VectorToPosition.X) + -1.6f;

                // Manually create a rotation matrix
                var cos = (float) Math.Cos(angle);
                var sin = (float) Math.Sin(angle);
                float[,] rotationMatrix = { { cos, -sin }, { sin, cos } };

                // Rotate each vertex
                for (var i = 0; i < triangleShapeVectorPoints.Length; i++)
                {
                    var vertex = triangleShapeVectorPoints[i];
                    var x = vertex.X * rotationMatrix[0, 0] + vertex.Y * rotationMatrix[0, 1];
                    var y = vertex.X * rotationMatrix[1, 0] + vertex.Y * rotationMatrix[1, 1];
                    triangleShapeVectorPoints[i] = new Vector2(x, y);
                }
            }

            var triangleCenterVector =
                (triangleShapeVectorPoints[0] + triangleShapeVectorPoints[1] + triangleShapeVectorPoints[2]) / 3;

            // Calculate the vectors from the center to each vertex
            var vectorsFromCenter = new Vector2[3];
            for (int i = 0; i < 3; i++)
            {
                vectorsFromCenter[i] = (triangleShapeVectorPoints[i] - triangleCenterVector) * UIScale;
            }

            // Calculate the vertices of the new triangle
            var newVerts = new Vector2[3];
            for (var i = 0; i < 3; i++)
            {
                newVerts[i] = (blipData.UiPosition * UIScale) + vectorsFromCenter[i];
            }

            if (!blipValueList.TryGetValue(blipData.Color, out var valueList))
            {
                valueList = new ValueList<Vector2>();
            }

            valueList.Add(newVerts[0]);
            valueList.Add(newVerts[1]);
            valueList.Add(newVerts[2]);
            blipValueList[blipData.Color] = valueList;
        }

        // One draw call for every color we have
        foreach (var color in blipValueList)
        {
            handle.DrawPrimitives(DrawPrimitiveTopology.TriangleList, color.Value.Span, color.Key);
        }
    }
}
