// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityTable.EntitySelectors;

namespace Content.Goobstation.Shared.Fishing.Components;

[RegisterComponent]
public sealed partial class FishingSpotComponent : Component
{
    /// <summary>
    /// All possible fishes to catch here
    /// </summary>
    [DataField(required: true)]
    public EntityTableSelector FishList;

    /// <summary>
    /// Default time for fish to occur
    /// </summary>
    [DataField]
    public float FishDefaultTimer;

    /// <summary>
    /// Variety number that FishDefaultTimer can go up or down to randomly
    /// </summary>
    [DataField]
    public float FishTimerVariety;
}
