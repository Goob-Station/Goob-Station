// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared.Humanoid
{
    [Serializable, NetSerializable]
    public enum HumanoidVisualLayers : byte
    {
        Special, // for the cat ears
        Tail,
        Wings, // For IPC wings porting from SimpleStation
        Hair,
        FacialHair,
        UndergarmentTop,
        UndergarmentBottom,
        Face, // EE Plasmeme Port
        Chest,
        Groin,
        Head,
        Snout,
        SnoutCover, // things layered over snouts (i.e. noses)
        HeadSide, // side parts (i.e., frills)
        HeadTop,  // top parts (i.e., ears)
        Eyes,
        RArm,
        LArm,
        RHand,
        LHand,
        RLeg,
        LLeg,
        RFoot,
        LFoot,
        Handcuffs,
        StencilMask,
        Ensnare,
        Fire,

    }
}
