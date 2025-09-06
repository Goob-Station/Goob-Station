// SPDX-FileCopyrightText: 2025 Evaisa <mail@evaisa.dev>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Shared.FloorGoblin;

[RegisterComponent]
public sealed partial class CollectShoesConditionComponent : Component
{
    [DataField]
    public int Required;

    [DataField]
    public int Collected;

    [DataField]
    public int Min = 3;

    [DataField]
    public int Max = 20;

    [DataField]
    public int Base = 1;

    [DataField]
    public float PerPlayer = 0.25f;
}
