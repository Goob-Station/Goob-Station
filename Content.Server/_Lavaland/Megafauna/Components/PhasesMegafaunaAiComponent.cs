// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;
using Content.Server._Lavaland.Megafauna.Systems;

namespace Content.Server._Lavaland.Megafauna.Components;

/// <summary>
/// Calculates current phase of the boss using damage thresholds.
/// Phases are counted from 1 and then go in PhaseThresholds order.
///
/// This number can be used in PhasesMegafaunaCondition to get
/// some attacks to be picked only at specific phases.
/// </summary>
[RegisterComponent, Access(typeof(MegafaunaSystem))]
public sealed partial class PhasesMegafaunaAiComponent : Component
{
    [DataField]
    public int CurrentPhase = 1;

    /// <summary>
    /// If true, when the boss heals the damage, allows them to switch to a previous phase.
    /// </summary>
    [DataField]
    public bool CanSwitchBack;

    /// <summary>
    /// At which damage this megafauna switches phases.
    /// </summary>
    [ViewVariables]
    public Dictionary<FixedPoint2, int> PhaseThresholds = new();

    /// <summary>
    /// Stores unscaled phase values that were set on MapInit.
    /// </summary>
    [DataField("phaseThresholds", required: true)]
    public Dictionary<FixedPoint2, int> BasePhaseThresholds;

    [ViewVariables]
    public float UpdateAccumulator = 2f;

    [DataField]
    public float UpdatePeriod = 2f;
}
