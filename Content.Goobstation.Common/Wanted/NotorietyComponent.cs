// SPDX-FileCopyrightText: 2026 Goob-Station Contributors <https://github.com/Goob-Station/Goob-Station>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Goobstation.Common.Wanted;

/// <summary>
/// Tracks a crew member's notoriety — accumulated over time as they commit crimes and become marked as wanted.
/// Notoriety persists even after criminal status is cleared, and decays slowly over time while the criminal lays low.
/// High notoriety can trigger a station-wide manhunt event.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class NotorietyComponent : Component
{
    public const int MaxLevel = 5;

    /// <summary>
    /// Credit bounties offered per notoriety tier (indexed by level 0–5).
    /// </summary>
    public static readonly int[] BountyByLevel = [0, 500, 1500, 3500, 7000, 12000];

    /// <summary>
    /// Current notoriety level. 0 = clean, 5 = most wanted.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int Level = 0;

    /// <summary>
    /// Timestamp of the last escalation (status set to Wanted/Dangerous/Perma).
    /// Used to calculate when notoriety should begin to decay.
    /// </summary>
    [DataField]
    public TimeSpan LastEscalationTime = TimeSpan.Zero;

    /// <summary>
    /// Total number of notoriety escalations this entity has accumulated this round.
    /// </summary>
    [DataField]
    public int TotalEscalations = 0;

    /// <summary>
    /// Credit bounty for the entity's current notoriety level.
    /// </summary>
    public int BountyAmount => Level is >= 0 and <= MaxLevel ? BountyByLevel[Level] : 0;
}
