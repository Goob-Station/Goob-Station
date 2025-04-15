// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2022 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Kevin Zheng <kevinz5000@gmail.com>
// SPDX-FileCopyrightText: 2024 0x6273 <0x40@keemail.me>
// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 DEATHB4DEFEAT <77995199+DEATHB4DEFEAT@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.CCVar;
using Content.Shared.CCVar;
using Robust.Shared.Configuration;

namespace Content.Server.Atmos.EntitySystems
{
    public sealed partial class AtmosphereSystem
    {
        [Dependency] private readonly IConfigurationManager _cfg = default!;

        public bool SpaceWind { get; private set; }
        public float SpaceWindPressureForceDivisorThrow { get; private set; }
        public float SpaceWindPressureForceDivisorPush { get; private set; }
        public float SpaceWindMaxVelocity { get; private set; }
        public float SpaceWindMaxPushForce { get; private set; }
        public float SpaceWindMinimumCalculatedMass { get; private set; } // Goobstation - Spacewind Cvars
        public float SpaceWindMaximumCalculatedInverseMass { get; private set; } // Goobstation - Spacewind Cvars
        public bool MonstermosUseExpensiveAirflow { get; private set; } // Goobstation - Spacewind Cvars
        public bool MonstermosEqualization { get; private set; }
        public bool MonstermosDepressurization { get; private set; }
        public bool MonstermosRipTiles { get; private set; }
        public float MonstermosRipTilesMinimumPressure { get; private set; }
        public float MonstermosRipTilesPressureOffset { get; private set; }
        public bool GridImpulse { get; private set; }
        public float SpacingEscapeRatio { get; private set; }
        public float SpacingMinGas { get; private set; }
        public float SpacingMaxWind { get; private set; }
        public bool Superconduction { get; private set; }
        public bool ExcitedGroups { get; private set; }
        public bool ExcitedGroupsSpaceIsAllConsuming { get; private set; }
        public float AtmosMaxProcessTime { get; private set; }
        public float AtmosTickRate { get; private set; }
        public float Speedup { get; private set; }
        public float HeatScale { get; private set; }
        public float HumanoidThrowMultiplier { get; private set; }

        /// <summary>
        /// Time between each atmos sub-update.  If you are writing an atmos device, use AtmosDeviceUpdateEvent.dt
        /// instead of this value, because atmos devices do not update each are sub-update and sometimes are skipped to
        /// meet the tick deadline.
        /// </summary>
        public float AtmosTime => 1f / AtmosTickRate;

        private void InitializeCVars()
        {
            Subs.CVar(_cfg, CCVars.SpaceWind, value => SpaceWind = value, true);
            Subs.CVar(_cfg, CCVars.SpaceWindPressureForceDivisorThrow, value => SpaceWindPressureForceDivisorThrow = value, true);
            Subs.CVar(_cfg, CCVars.SpaceWindPressureForceDivisorPush, value => SpaceWindPressureForceDivisorPush = value, true);
            Subs.CVar(_cfg, CCVars.SpaceWindMaxVelocity, value => SpaceWindMaxVelocity = value, true);
            Subs.CVar(_cfg, CCVars.SpaceWindMaxPushForce, value => SpaceWindMaxPushForce = value, true);
            Subs.CVar(_cfg, GoobCVars.SpaceWindMinimumCalculatedMass, value => SpaceWindMinimumCalculatedMass = value, true); // Goobstation - Spacewind Cvars
            Subs.CVar(_cfg, GoobCVars.SpaceWindMaximumCalculatedInverseMass, value => SpaceWindMaximumCalculatedInverseMass = value, true); // Goobstation - Spacewind Cvars
            Subs.CVar(_cfg, GoobCVars.MonstermosUseExpensiveAirflow, value => MonstermosUseExpensiveAirflow = value, true); // Goobstation - Spacewind Cvars
            Subs.CVar(_cfg, CCVars.MonstermosEqualization, value => MonstermosEqualization = value, true);
            Subs.CVar(_cfg, CCVars.MonstermosDepressurization, value => MonstermosDepressurization = value, true);
            Subs.CVar(_cfg, CCVars.MonstermosRipTiles, value => MonstermosRipTiles = value, true);
            Subs.CVar(_cfg, GoobCVars.MonstermosRipTilesMinimumPressure, value => MonstermosRipTilesMinimumPressure = value, true);
            Subs.CVar(_cfg, GoobCVars.MonstermosRipTilesPressureOffset, value => MonstermosRipTilesPressureOffset = value, true);
            Subs.CVar(_cfg, CCVars.AtmosGridImpulse, value => GridImpulse = value, true);
            Subs.CVar(_cfg, CCVars.AtmosSpacingEscapeRatio, value => SpacingEscapeRatio = value, true);
            Subs.CVar(_cfg, CCVars.AtmosSpacingMinGas, value => SpacingMinGas = value, true);
            Subs.CVar(_cfg, CCVars.AtmosSpacingMaxWind, value => SpacingMaxWind = value, true);
            Subs.CVar(_cfg, CCVars.Superconduction, value => Superconduction = value, true);
            Subs.CVar(_cfg, CCVars.AtmosMaxProcessTime, value => AtmosMaxProcessTime = value, true);
            Subs.CVar(_cfg, CCVars.AtmosTickRate, value => AtmosTickRate = value, true);
            Subs.CVar(_cfg, CCVars.AtmosSpeedup, value => Speedup = value, true);
            Subs.CVar(_cfg, CCVars.AtmosHeatScale, value => { HeatScale = value; InitializeGases(); }, true);
            Subs.CVar(_cfg, CCVars.ExcitedGroups, value => ExcitedGroups = value, true);
            Subs.CVar(_cfg, CCVars.ExcitedGroupsSpaceIsAllConsuming, value => ExcitedGroupsSpaceIsAllConsuming = value, true);
            Subs.CVar(_cfg, GoobCVars.AtmosHumanoidThrowMultiplier, value => HumanoidThrowMultiplier = value, true);
        }
    }
}