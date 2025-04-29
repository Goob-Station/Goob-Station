// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Fishing.Components;

/// <summary>
/// Dynamic component, that is assigned to active fishing spots that are currently waiting for da fish.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ActiveFishingSpotComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public EntityUid? AttachedFishingLure;

    [DataField, AutoNetworkedField]
    public TimeSpan? FishingStartTime;

    /// <summary>
    /// If true, someone is pulling fish out of this spot.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsActive;

    [DataField, AutoNetworkedField]
    public float FishDifficulty;

    /// <summary>
    /// Fish that we're currently trying to catch
    /// </summary>
    [DataField]
    public EntProtoId? Fish; // not networked because useless for client
}
