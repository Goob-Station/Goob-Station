// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.StationEvents;
using Content.Shared.Random;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.StationEvents.Components;

[RegisterComponent, Access(typeof(GameDirectorSystem))]
public sealed partial class GameDirectorComponent : Component
{
    /// <summary>
    ///   How long until the next check for an event runs
    ///   Default value is how long until first event is allowed
    /// </summary>
    [DataField]
    public TimeSpan TimeNextEvent;

    /// <summary>
    ///   Used to restrict the game director to spawning positive events for some time after high-impact negative events
    /// </summary>
    [DataField]
    public TimeSpan EventIntervalMin = TimeSpan.FromMinutes(2);

    /// <summary>
    ///   Used to restrict the game director to spawning positive events for some time after high-impact negative events
    /// </summary>
    [DataField]
    public TimeSpan EventIntervalMax = TimeSpan.FromMinutes(8);

    /// <summary>
    ///   The current chaos score
    ///   Lower values will tend to spawn higher-impact events
    ///   Can (and often will be) be negative
    /// </summary>
    [DataField]
    public float ChaosScore = 0;

    /// <summary>
    /// The minimum amount of chaos worth of events to generate at the start, per player.
    /// </summary>
    [DataField]
    public float MinStartingChaos = 5;

    /// <summary>
    /// The maximum amount of chaos worth of events to generate at the start, per player.
    /// </summary>
    [DataField]
    public float MaxStartingChaos = 10;

    /// <summary>
    ///   How much to change chaos per second per living person
    /// </summary>
    [DataField]
    public float LivingChaosChange = -0.005f;

    /// <summary>
    ///   How much to change chaos per second per dead person
    /// </summary>
    [DataField]
    public float DeadChaosChange = 0.01f;

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
    public float ChaosExponent = 1.5f;

    /// <summary>
    ///   Higher values make the game director be more picky with events.
    /// </summary>
    [DataField]
    public float ChaosMatching = 2f;

    /// <summary>
    /// Does this round start with antags at all?
    /// </summary>
    [DataField]
    public bool NoRoundstartAntags;

    /// <summary>
    /// Whether to ignore whether an event can actually run in this context.
    /// </summary>
    [DataField]
    public bool IgnoreTimings = false;

    /// <summary>
    ///   All the event types that are disallowed to run in the current rule
    /// </summary>
    [DataField]
    public HashSet<ProtoId<EventTypePrototype>> DisallowedEvents = new();

    [ViewVariables]
    public List<SelectedEvent> SelectedEvents = new();

    /// <summary>
    /// All the possible roundstart antags.
    /// </summary>
    public ProtoId<WeightedRandomPrototype> RoundStartAntagsWeightTable = "GameDirector";
}
