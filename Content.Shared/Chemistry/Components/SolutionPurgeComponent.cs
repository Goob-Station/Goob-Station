// SPDX-FileCopyrightText: 2023 Arimah <arimah42@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 EnDecc <33369477+Endecc@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Chemistry.Reagent;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Chemistry.EntitySystems;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Chemistry.Components;

/// <summary>
/// Passively decreases a solution's quantity of reagent(s).
/// </summary>
[RegisterComponent, AutoGenerateComponentPause]
[NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SolutionPurgeSystem))]
public sealed partial class SolutionPurgeComponent : Component
{
    /// <summary>
    /// The name of the solution to detract from.
    /// </summary>
    [DataField(required: true)]
    public string Solution = string.Empty;

    /// <summary>
    /// The reagent(s) to be ignored when purging the solution
    /// </summary>
    [DataField]
    public List<ProtoId<ReagentPrototype>> Preserve = [];

    /// <summary>
    /// Amount of reagent(s) that are purged
    /// </summary>
    [DataField(required: true)]
    public FixedPoint2 Quantity;

    /// <summary>
    /// How long it takes to purge once.
    /// </summary>
    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(1);

    /// <summary>
    /// The time when the next purge will occur.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField, AutoNetworkedField]
    public TimeSpan NextPurgeTime;
}
