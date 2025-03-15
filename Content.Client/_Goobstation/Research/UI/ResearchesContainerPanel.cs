using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;
using System.Linq;
using System.Numerics;

namespace Content.Client._Goobstation.Research.UI;

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
                var leftOffset = second.PixelPosition.Y;
                var rightOffset = item.PixelPosition.Y;

                var y1 = second.PixelPosition.Y + second.PixelHeight / 2 + leftOffset;
                var y2 = item.PixelPosition.Y + item.PixelHeight / 2 + rightOffset;

                if (second == item)
                {
                    handle.DrawLine(new Vector2(0, y1), new Vector2(PixelWidth, y2), Color.Cyan);
                    continue;
                }
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
