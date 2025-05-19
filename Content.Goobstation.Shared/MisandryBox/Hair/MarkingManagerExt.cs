// SPDX-FileCopyrightText: 2025 Conchelle <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.MisandryBox.Hair;

public static class MarkingManagerExtensions
{
    public static bool HasHair(this MarkingManager mark, ProtoId<SpeciesPrototype> specie)
    {
        var hairstyles = mark.MarkingsByCategoryAndSpecies(MarkingCategories.Hair, specie).Keys.ToList();
        return hairstyles.Count != 0;
    }
}
