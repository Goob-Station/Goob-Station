// SPDX-FileCopyrightText: 2021 E F R <602406+Efruit@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 ike709 <ike709@github.com>
// SPDX-FileCopyrightText: 2021 ike709 <ike709@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;

namespace Content.Shared.Atmos.Piping.Trinary.Components
{
    [Serializable, NetSerializable]
    public enum GasMixerUiKey
    {
        Key,
    }

    [Serializable, NetSerializable]
    public sealed class GasMixerBoundUserInterfaceState : BoundUserInterfaceState
    {
        public string MixerLabel { get; }
        public float OutputPressure { get; }
        public bool Enabled { get; }

        public float NodeOne { get; }

        public GasMixerBoundUserInterfaceState(string mixerLabel, float outputPressure, bool enabled, float nodeOne)
        {
            MixerLabel = mixerLabel;
            OutputPressure = outputPressure;
            Enabled = enabled;
            NodeOne = nodeOne;
        }
    }

    [Serializable, NetSerializable]
    public sealed class GasMixerToggleStatusMessage : BoundUserInterfaceMessage
    {
        public bool Enabled { get; }

        public GasMixerToggleStatusMessage(bool enabled)
        {
            Enabled = enabled;
        }
    }

    [Serializable, NetSerializable]
    public sealed class GasMixerChangeOutputPressureMessage : BoundUserInterfaceMessage
    {
        public float Pressure { get; }

        public GasMixerChangeOutputPressureMessage(float pressure)
        {
            Pressure = pressure;
        }
    }

    [Serializable, NetSerializable]
    public sealed class GasMixerChangeNodePercentageMessage : BoundUserInterfaceMessage
    {
        public float NodeOne { get; }

        public GasMixerChangeNodePercentageMessage(float nodeOne)
        {
            NodeOne = nodeOne;
        }
    }
}