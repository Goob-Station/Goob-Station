// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Plykiya <plykiya@protonmail.com>
// SPDX-FileCopyrightText: 2024 Preston Smith <92108534+thetolbean@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 blueDev2 <89804215+blueDev2@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 osjarw <62134478+osjarw@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 veprolet <68151557+veprolet@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Prototypes;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.DoAfter;
using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Chemistry.Components;

/// <summary>
/// Implements draw/inject behavior for droppers and syringes.
/// </summary>
/// <remarks>
/// Can optionally support both
/// injection and drawing or just injection. Can inject/draw reagents from solution
/// containers, and can directly inject into a mob's bloodstream.
/// </remarks>
/// <seealso cref="InjectorModePrototype"/>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(InjectorSystem))]
public sealed partial class InjectorComponent : Component
{
    /// <summary>
    /// The solution to draw into or inject from.
    /// </summary>
    [DataField]
    public string SolutionName = "injector";

    /// <summary>
    /// A cached reference to the solution.
    /// </summary>
    [ViewVariables]
    public Entity<SolutionComponent>? Solution = null;

    /// <summary>
    /// Amount to inject or draw on each usage.
    /// </summary>
    /// <remarks>
    /// If its set null, this injector is marked to inject its entire contents upon usage.
    /// </remarks>
    [DataField, AutoNetworkedField]
    public FixedPoint2? CurrentTransferAmount = FixedPoint2.New(5);


    /// <summary>
    /// The mode that this injector starts with on MapInit.
    /// </summary>
    [DataField(required: true), AutoNetworkedField]
    public ProtoId<InjectorModePrototype> ActiveModeProtoId;

    /// <summary>
    /// The possible <see cref="InjectorModePrototype"/> that it can switch between.
    /// </summary>
    [DataField(required: true)]
    public List<ProtoId<InjectorModePrototype>> AllowedModes;

    /// <summary>
    /// Whether the injector is able to draw from or inject from mobs.
    /// </summary>
    /// <example>
    /// Droppers ignore mobs.
    /// </example>
    [DataField]
    public bool IgnoreMobs;

    /// <summary>
    /// Whether the injector is able to draw from or inject into containers that are closed/sealed.
    /// </summary>
    /// <example>
    /// Droppers can't inject into closed cans.
    /// </example>
    [DataField]
    public bool IgnoreClosed = true;

    /// <summary>
    /// Reagents that are allowed to be within this injector.
    /// If a solution has both allowed and non-allowed reagents, only allowed reagents will be drawn into this injector.
    /// A null ReagentWhitelist indicates all reagents are allowed.
    /// </summary>
    [DataField]
    public List<ProtoId<ReagentPrototype>>? ReagentWhitelist;

    #region Arguments for injection doafter

    /// <inheritdoc cref="DoAfterArgs.NeedHand"/>
    [DataField]
    public bool NeedHand = true;

    /// <inheritdoc cref="DoAfterArgs.BreakOnHandChange"/>
    [DataField]
    public bool BreakOnHandChange = true;

    /// <inheritdoc cref="DoAfterArgs.MovementThreshold"/>
    [DataField]
    public float MovementThreshold = 0.1f;

    #endregion
}

internal static class InjectorToggleModeExtensions
{
    public static bool HasAnyFlag(this InjectorBehavior s1, InjectorBehavior s2)
    {
        return (s1 & s2) != 0;
    }
}
