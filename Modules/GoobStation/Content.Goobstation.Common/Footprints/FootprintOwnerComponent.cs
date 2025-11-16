// SPDX-FileCopyrightText: 2025 BombasterDS <deniskaporoshok@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Common.Footprints;

[RegisterComponent]
public sealed partial class FootprintOwnerComponent : Component
{
    [DataField]
    public float MaxFootVolume = 10;

    [DataField]
    public float MaxBodyVolume = 20;

    [DataField]
    public float MinFootprintVolume = 0.5f;

    [DataField]
    public float MaxFootprintVolume = 1;

    [DataField]
    public float MinBodyprintVolume = 2;

    [DataField]
    public float MaxBodyprintVolume = 5;

    [DataField]
    public float FootDistance = 0.5f;

    [DataField]
    public float BodyDistance = 1;

    [ViewVariables(VVAccess.ReadWrite)]
    public float Distance;

    [DataField]
    public float NextFootOffset = 0.0625f;
}
