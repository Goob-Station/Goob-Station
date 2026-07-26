// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared.Humanoid.Markings
{
    [Serializable, NetSerializable]
    public enum MarkingCategories : byte
    {
        Special,
        Hair,
        FacialHair,
        Head,
        HeadTop,
        HeadSide,
        Face, // Plasmeme Port
        Snout,
        SnoutCover,
        Chest,
        UndergarmentTop,
        UndergarmentBottom,
        RightArm,
        RightHand,
        LeftArm,
        LeftHand,
        RightLeg,
        RightFoot,
        LeftLeg,
        LeftFoot,
        Arms,
        Legs,
        Groin, // Shitmed Change
        Wings, // For IPC wings porting from SimpleStation
        Tail,
        Overlay
    }

    public static class MarkingCategoriesConversion
    {
        public static MarkingCategories FromHumanoidVisualLayers(HumanoidVisualLayers layer)
        {
            return layer switch
            {
                HumanoidVisualLayers.Special => MarkingCategories.Special,
                HumanoidVisualLayers.Face => MarkingCategories.Face, // Plasmeme Port
                HumanoidVisualLayers.Hair => MarkingCategories.Hair,
                HumanoidVisualLayers.FacialHair => MarkingCategories.FacialHair,
                HumanoidVisualLayers.Head => MarkingCategories.Head,
                HumanoidVisualLayers.HeadTop => MarkingCategories.HeadTop,
                HumanoidVisualLayers.HeadSide => MarkingCategories.HeadSide,
                HumanoidVisualLayers.Snout => MarkingCategories.Snout,
                HumanoidVisualLayers.Chest => MarkingCategories.Chest,
                HumanoidVisualLayers.UndergarmentTop => MarkingCategories.UndergarmentTop,
                HumanoidVisualLayers.UndergarmentBottom => MarkingCategories.UndergarmentBottom,
                HumanoidVisualLayers.Groin => MarkingCategories.Groin, // Shitmed Change
                HumanoidVisualLayers.RArm => MarkingCategories.RightArm, // Goobstation
                HumanoidVisualLayers.LArm => MarkingCategories.LeftArm, // Goobstation
                HumanoidVisualLayers.RHand => MarkingCategories.RightHand, // Goobstation
                HumanoidVisualLayers.LHand => MarkingCategories.LeftHand, // Goobstation
                HumanoidVisualLayers.LLeg => MarkingCategories.LeftLeg, // Goobstation
                HumanoidVisualLayers.RLeg => MarkingCategories.RightLeg, // Goobstation
                HumanoidVisualLayers.LFoot => MarkingCategories.LeftFoot, // Goobstation
                HumanoidVisualLayers.RFoot => MarkingCategories.RightFoot, // Goobstation
                HumanoidVisualLayers.Wings => MarkingCategories.Wings, // Goobstation
                HumanoidVisualLayers.Tail => MarkingCategories.Tail,
                _ => MarkingCategories.Overlay
            };
        }
    }
}
