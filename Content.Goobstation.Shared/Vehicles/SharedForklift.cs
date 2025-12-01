// SPDX-FileCopyrightText...
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Vehicles;

[Serializable, NetSerializable]
public enum ForkliftVisuals : byte
{
    CrateState,
}

[Serializable, NetSerializable]
public enum ForkliftCrateState : byte
{
    Empty,
    OneCrate,
    TwoCrates,
    ThreeCrates,
    FourCrates,
}
