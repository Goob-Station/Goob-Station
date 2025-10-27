// SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;

namespace Content.Shared.Wires
{
    [Serializable, NetSerializable]
    public enum WireVisVisuals
    {
        ConnectedMask
    }

    [Flags]
    [Serializable, NetSerializable]
    public enum WireVisDirFlags : byte
    {
        None = 0,
        North = 1,
        South = 2,
        East = 4,
        West = 8
    }
}