// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Botany;

/// <summary>
/// Appearance data for the fermenting barrel: whether its lid is off.
/// </summary>
[Serializable, NetSerializable]
public enum FermentingBarrelVisuals : byte
{
    Open,
}
