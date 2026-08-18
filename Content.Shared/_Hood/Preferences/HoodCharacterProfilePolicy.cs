// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._Hood.Preferences;

/// <summary>
/// Defines which species can be stored in a normal Hood player profile.
/// Explicit species APIs remain available for special roles and admin-spawned characters.
/// </summary>
public static class HoodCharacterProfilePolicy
{
    public static readonly ProtoId<SpeciesPrototype> NormalSpecies = SharedHumanoidAppearanceSystem.DefaultSpecies;

    public static bool IsNormalCreationSpecies(ProtoId<SpeciesPrototype> species)
    {
        return species == NormalSpecies;
    }

    public static bool IsNormalCreationSpecies(string species)
    {
        return species == NormalSpecies.Id;
    }
}
