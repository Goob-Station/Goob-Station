// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared.Eye
{
    [Flags]
    [FlagsFor(typeof(VisibilityMaskLayer))]
    public enum VisibilityFlags : int
    {
        // GOOB EDIT: I've put the admin visflag at the top of everything, so admintools can see everything
        // Default wiz is like 8
        None = 0,
        Normal = 1 << 0,
        Ghost = 1 << 1, // Observers and revenants.
        Subfloor = 1 << 2, // Pipes, disposal chutes, cables etc. while hidden under tiles. Can be revealed with a t-ray.
        Abductor = 1 << 3, // Shitmed Change - Starlight Abductor
        CosmicCultMonument = 1 << 4, // DeltaV - DeltaV - Cosmic Cult
        EldritchInfluence = 1 << 5, // Goobstation
        EldritchInfluenceSpent = 1 << 6, // Goobstation
        Admin = 1 << 7, // Reserved for admins in stealth mode and admin tools.
    }
}
