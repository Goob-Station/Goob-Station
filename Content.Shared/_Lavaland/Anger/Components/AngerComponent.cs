// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.Anger.Components;

/// <summary>
/// Makes megafauna stronger when it takes more damage.
/// Aggression value can be used in MegafaunaActions to control their power.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AngerComponent : Component
{
    /// <summary>
    /// Current percentage of anger. By just receiving damage goes up to 1,
    /// but can be
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public float CurrentAnger = 0f;

    /// <summary>
    /// Total HP of a boss.
    /// </summary>
    [DataField(required: true), AutoNetworkedField]
    public FixedPoint2 TotalHp = 1;

    /// <summary>
    /// Minimal amount of anger.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float DefaultMinAnger;

    /// <summary>
    /// Soft-cap for anger that can be obtained with low HP.
    /// Other sources can make it even higher than this value.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float DefaultMaxAnger = 1f;

    /// <summary>
    /// Hard-cap for anger, that no matter what cannot be overcome.
    /// </summary>
    [DataField]
    public float AngerHardcap = 5f;

    /// <summary>
    /// Dynamic minimum anger that can be scaled by other sources.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public float MinAnger;

    /// <summary>
    /// Dynamic maximum anger that can be scaled by other sources.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public float MaxAnger;
}
