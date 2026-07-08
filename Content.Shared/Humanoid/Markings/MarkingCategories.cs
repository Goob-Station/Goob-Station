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
        Chest,
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
        Underwear,
        Undershirt,
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
                HumanoidVisualLayers.Undershirt => MarkingCategories.Undershirt,
                HumanoidVisualLayers.Underwear => MarkingCategories.Underwear,
                HumanoidVisualLayers.Chest => MarkingCategories.Chest,
                HumanoidVisualLayers.Groin => MarkingCategories.Groin, // Shitmed Change
                HumanoidVisualLayers.RArm => MarkingCategories.RightArm,
                HumanoidVisualLayers.LArm => MarkingCategories.LeftArm,
                HumanoidVisualLayers.RHand => MarkingCategories.RightHand,
                HumanoidVisualLayers.LHand => MarkingCategories.LeftHand,
                HumanoidVisualLayers.LLeg => MarkingCategories.LeftLeg,
                HumanoidVisualLayers.RLeg => MarkingCategories.RightLeg,
                HumanoidVisualLayers.LFoot => MarkingCategories.LeftFoot,
                HumanoidVisualLayers.RFoot => MarkingCategories.RightFoot,
                HumanoidVisualLayers.Wings => MarkingCategories.Wings,
                HumanoidVisualLayers.Tail => MarkingCategories.Tail,
                _ => MarkingCategories.Overlay
            };
        }
    }
}
