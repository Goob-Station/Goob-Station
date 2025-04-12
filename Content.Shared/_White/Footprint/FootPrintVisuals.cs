// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Spatison <137375981+Spatison@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared._White.FootPrint;

[Serializable, NetSerializable]
public enum FootPrintVisuals : byte
{
    BareFootPrint,
    ShoesPrint,
    SuitPrint,
    Dragging
}

[Serializable, NetSerializable]
public enum FootPrintVisualState : byte
{
    State,
    Color
}

[Serializable, NetSerializable]
public enum FootPrintVisualLayers : byte
{
    Print
}