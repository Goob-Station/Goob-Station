// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.StationEvents.SecretPlus;
using Content.Shared.Random;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.StationEvents.SecretPlus;

[RegisterComponent, Access(typeof(SecretPlusSystem))]
public sealed partial class SecretPlusComponent : Component
{
    /// <summary>
    ///   How long until the next check for an event runs
    ///   Default value is how long until first event is allowed
    /// </summary>
    [DataField]
    public TimeSpan TimeNextEvent;

    /// <summary>
    ///   Minimum interval between events
    /// </summary>
    [DataField]
    public TimeSpan EventIntervalMin;

    /// <summary>
    ///   Maximum interval between events
    /// </summary>
    [DataField]
    public TimeSpan EventIntervalMax;

    /// <summary>
    ///   The current chaos score
    ///   Lower values will tend to spawn higher-impact events
    ///   Can be negative
    /// </summary>
    [DataField]
    public float ChaosScore = 0;

    /// <summary>
    /// The minimum amount of chaos worth of roundstart antags to generate at the start, per player.
    /// </summary>
    [DataField]
    public float MinStartingChaos;

    /// <summary>
    /// The maximum amount of chaos worth of roundstart antags to generate at the start, per player.
    /// </summary>
    [DataField]
    public float MaxStartingChaos;

    /// <summary>
    ///   How much to change chaos per second per living person
    /// </summary>
    [DataField]
    public float LivingChaosChange;

    /// <summary>
    ///   How much to change chaos per second per dead person
    /// </summary>
    [DataField]
    public float DeadChaosChange;

    /// <summary>
    ///   How much to offset chaos of events away from 0 when picking events
    ///   Higher values make low-chaos events have more equal chances to be picked
    /// </summary>
    [DataField]
    public float ChaosOffset = 50f;

    /// <summary>
    ///   Higher values make high-chaos events more uncommon.
    /// </summary>
    [DataField]
    public float ChaosExponent = 1.2f;

    /// <summary>
    ///   Lower values make the game director be more picky with events.
    /// </summary>
    [DataField]
    public float ChaosMatching = 2f;

    /// <summary>
    ///   "Base" chaos value to use for event weighting.
    ///   Matters for how much having negative weight affects probability.
    /// </summary>
    [DataField]
    public float ChaosThreshold = 10f;

    /// <summary>
    /// Does this round start with antags at all?
    /// </summary>
    [DataField]
    public bool NoRoundstartAntags = false;

    /// <summary>
    /// Whether to ignore whether an event can actually run in this context.
    /// </summary>
    [DataField]
    public bool IgnoreTimings = false;

    /// <summary>
    /// Whether to ignore incompatible roundstart antags. Also permits several of one antag.
    /// </summary>
    [DataField]
    public bool IgnoreIncompatible = false;

    /// <summary>
    ///   All the event types that are disallowed to run in the current rule
    /// </summary>
    [DataField]
    public HashSet<ProtoId<EventTypePrototype>> DisallowedEvents = new();

    /// <summary>
    ///   Cache for currently runnable events.
    [ViewVariables]
    public List<SelectedEvent> SelectedEvents = new();

    /// <summary>
    /// Weight table for primary roundstart antags.
    /// </summary>
    [DataField]
    public ProtoId<WeightedRandomPrototype> PrimaryAntagsWeightTable = "SecretPlusPrimary";

    /// <summary>
    ///   Makes the primary antag gamerule be less likely to be skipped due to lacking chaos budget.
    /// </summary>
    [DataField]
    public float PrimaryAntagChaosBias = 2f;

    /// <summary>
    /// Weight table for roundstart antags.
    /// </summary>
    [DataField]
    public ProtoId<WeightedRandomPrototype> RoundStartAntagsWeightTable = "SecretPlus";
}
