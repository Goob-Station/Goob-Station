// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Robust.Shared.Serialization;

namespace Content.Shared.Fax;

[Serializable, NetSerializable]
public enum FaxMachineVisuals : byte
{
    VisualState,
}

[Serializable, NetSerializable]
public enum FaxMachineVisualState : byte
{
    Normal,
    Inserting,
    Printing
}
