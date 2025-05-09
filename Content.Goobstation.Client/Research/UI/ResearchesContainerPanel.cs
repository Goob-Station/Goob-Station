// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 FaDeOkno <143940725+FaDeOkno@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 FaDeOkno <logkedr18@gmail.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;
using System.Linq;
using System.Numerics;

namespace Content.Goobstation.Client.Research.UI;

/// <summary>
/// UI element for visualizing technologies prerequisites
/// </summary>
public sealed partial class ResearchesContainerPanel : LayoutContainer
{
    public ResearchesContainerPanel()
    {
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        foreach (var child in Children)
        {
            if (child is not FancyResearchConsoleItem item)
                continue;

            if (item.Prototype.TechnologyPrerequisites.Count <= 0)
                continue;

            var list = Children.Where(x => x is FancyResearchConsoleItem second && item.Prototype.TechnologyPrerequisites.Contains(second.Prototype.ID));
            foreach (var second in list)
            {

                var startCoords = new Vector2(item.PixelPosition.X + item.PixelWidth / 2, item.PixelPosition.Y + item.PixelHeight / 2);
                var endCoords = new Vector2(second.PixelPosition.X + second.PixelWidth / 2, second.PixelPosition.Y + second.PixelHeight / 2);

                if (second.PixelPosition.Y != item.PixelPosition.Y)
                {

                    handle.DrawLine(startCoords, new(endCoords.X, startCoords.Y), Color.White);
                    handle.DrawLine(new(endCoords.X, startCoords.Y), endCoords, Color.White);
                }
                else
                {
                    handle.DrawLine(startCoords, endCoords, Color.White);
                }
            }
        }
    }
}