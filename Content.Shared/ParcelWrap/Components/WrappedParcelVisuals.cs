// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Robust.Shared.Serialization;

namespace Content.Shared.ParcelWrap.Components;

/// <summary>
/// This enum is used to change the sprite used by WrappedParcels based on the parcel's size.
/// </summary>
[Serializable, NetSerializable]
public enum WrappedParcelVisuals : byte
{
    Size,
    Layer,
}
