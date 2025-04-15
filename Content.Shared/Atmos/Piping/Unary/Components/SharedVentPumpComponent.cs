// SPDX-FileCopyrightText: 2022 E F R <602406+Efruit@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 vulppine <vulppine@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 MjrLandWhale <brandonemitch@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Atmos.Monitor.Components;
using Robust.Shared.Serialization;

namespace Content.Shared.Atmos.Piping.Unary.Components
{
    [Serializable, NetSerializable]
    public sealed class GasVentPumpData : IAtmosDeviceData
    {
        public bool Enabled { get; set; }
        public bool Dirty { get; set; }
        public bool IgnoreAlarms { get; set; } = false;
        public VentPumpDirection PumpDirection { get; set; } = VentPumpDirection.Releasing;
        public VentPressureBound PressureChecks { get; set; } = VentPressureBound.ExternalBound;
        public float ExternalPressureBound { get; set; } = Atmospherics.OneAtmosphere;
        public float InternalPressureBound { get; set; } = 0f;
        public bool PressureLockoutOverride { get; set; } = false;

        // Presets for 'dumb' air alarm modes

        public static GasVentPumpData FilterModePreset = new GasVentPumpData
        {
            Enabled = true,
            PumpDirection = VentPumpDirection.Releasing,
            PressureChecks = VentPressureBound.ExternalBound,
            ExternalPressureBound = Atmospherics.OneAtmosphere,
            InternalPressureBound = 0f,
            PressureLockoutOverride = false
        };

        public static GasVentPumpData FillModePreset = new GasVentPumpData
        {
            Enabled = true,
            Dirty = true,
            PumpDirection = VentPumpDirection.Releasing,
            PressureChecks = VentPressureBound.ExternalBound,
            ExternalPressureBound = Atmospherics.OneAtmosphere * 50,
            InternalPressureBound = 0f,
            PressureLockoutOverride = true
        };

        public static GasVentPumpData PanicModePreset = new GasVentPumpData
        {
            Enabled = false,
            Dirty = true,
            PumpDirection = VentPumpDirection.Releasing,
            PressureChecks = VentPressureBound.ExternalBound,
            ExternalPressureBound = Atmospherics.OneAtmosphere,
            InternalPressureBound = 0f,
            PressureLockoutOverride = false
        };

        public static GasVentPumpData ReplaceModePreset = new GasVentPumpData
        {
            Enabled = false,
            IgnoreAlarms = true,
            Dirty = true,
            PumpDirection = VentPumpDirection.Releasing,
            PressureChecks = VentPressureBound.ExternalBound,
            ExternalPressureBound = Atmospherics.OneAtmosphere,
            InternalPressureBound = 0f,
            PressureLockoutOverride = false
        };
    }

    [Serializable, NetSerializable]
    public enum VentPumpDirection : sbyte
    {
        Siphoning = 0,
        Releasing = 1,
    }

    [Flags]
    [Serializable, NetSerializable]
    public enum VentPressureBound : sbyte
    {
        NoBound       = 0,
        InternalBound = 1,
        ExternalBound = 2,
        Both = 3,
    }
}