// SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2022 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Singularity.Components
{
    /// <summary>
    /// Overrides exactly how much energy this object gives to a singularity.
    /// </summary>
    [RegisterComponent]
    public sealed partial class SinguloFoodComponent : Component
    {
        /// <summary>
        /// Flat adjustment to the singularity's energy when this entity is eaten by the event horizon.
        /// </summary>
        [DataField]
        public float Energy = 1f;

        /// <summary>
        /// Multiplier applied to singularity's energy.
        /// 1.0 = no change, 0.97 = 3% reduction, 1.05 = 5% increase
        /// </summary>
        /// /// <remarks>
        /// This is calculated using the singularity's energy level before <see cref="Energy"/> has been added.
        /// </remarks>
        [DataField]
        public float EnergyFactor = 1f;
    }
}