// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;

namespace Content.Goobstation.Shared.Nutrition.EntitySystems;

public sealed class GoobHungerSystem : EntitySystem
{
    [Dependency] private readonly HungerSystem _hunger = null!;

    /// <summary>
    /// A check that returns if the entity is below a hunger threshold. || Goobstation
    /// </summary>
    public bool IsHungerAboveState(EntityUid uid, HungerThreshold threshold, float? food = null, HungerComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return false; // It's never going to go hungry, so it's probably fine to assume that it's not... you know, hungry.

        return _hunger.GetHungerThreshold(comp, food) > threshold;
    }
}
