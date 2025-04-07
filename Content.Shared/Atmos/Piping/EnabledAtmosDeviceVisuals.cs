// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Menshin <Menshin@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;

namespace Content.Shared.Atmos.Piping
{
    [Serializable, NetSerializable]
    public enum OutletInjectorVisuals : byte
    {
        Enabled,
    }

    [Serializable, NetSerializable]
    public enum PassiveVentVisuals : byte
    {
        Enabled,
    }

    [Serializable, NetSerializable]
    public enum VentScrubberVisuals : byte
    {
        Enabled,
    }

    [Serializable, NetSerializable]
    public enum PumpVisuals : byte
    {
        Enabled,
    }

    [Serializable, NetSerializable]
    public enum FilterVisuals : byte
    {
        Enabled,
    }
}