// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared.Damage.Components
{
    // TODO It'd be nice if this could be a destructible threshold, but on the other hand,
    // that doesn't really work with events at all, and
    [RegisterComponent, NetworkedComponent]
    public sealed partial class SlowOnDamageComponent : Component
    {
        /// <summary>
        ///     Damage -> movespeed dictionary. This is -damage-, not -health-.
        /// </summary>
        [DataField("speedModifierThresholds", required: true)]
        public Dictionary<FixedPoint2, float> SpeedModifierThresholds = default!;
    }
}