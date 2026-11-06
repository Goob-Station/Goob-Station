using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Robust.Shared.Utility;

namespace Content.Goobstation.UIKit.UserInterface.Humanoid;

public static class HumanoidTileArt
{
    public static readonly UIBox2 FullBody = new(0, 0, 1, 1);
    public static readonly UIBox2 HeadRegion = new(0.25f, 0f, 0.75f, 0.5f);

    public static bool IsHeadCategory(MarkingCategories category)
    {
        return category is MarkingCategories.Head
            or MarkingCategories.HeadTop
            or MarkingCategories.HeadSide
            or MarkingCategories.Face
            or MarkingCategories.Snout
            or MarkingCategories.SnoutCover
            or MarkingCategories.Hair
            or MarkingCategories.FacialHair;
    }

    public static HumanoidVisualLayers SlotFor(MarkingPrototype proto, int sprite)
    {
        if (proto.Layering != null
            && proto.Sprites[sprite] is SpriteSpecifier.Rsi rsi
            && proto.Layering.TryGetValue(rsi.RsiState, out var layerName)
            && Enum.TryParse<HumanoidVisualLayers>(layerName, out var layer))
        {
            return layer;
        }

        return proto.BodyPart;
    }
}
