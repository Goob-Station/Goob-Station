// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._Lavaland.Megafauna.Actions;
using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.Megafauna.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MegafaunaAiComponent : Component
{
    [DataField(required: true), ViewVariables(VVAccess.ReadOnly)]
    public MegafaunaActionSelector Selector;

    [ViewVariables, AutoNetworkedField]
    public bool Active;

    [ViewVariables]
    public Dictionary<TimeSpan, MegafaunaActionSelector> ActionSchedule = new();

    /// <summary>
    /// Target that is picked before each new attack
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public EntityUid? CurrentTarget;

    [ViewVariables, AutoNetworkedField]
    public EntityUid? PreviousTarget;

    /// <summary>
    /// If true, will allow this boss to pick the same attacks twice in a row.
    /// </summary>
    [DataField]
    public bool CanRepeatAttacks;

    /// <summary>
    /// When the boss doesn't die ut for any reason stops attacking,
    /// if this bool is true, will rejuvenate the megafauna.
    /// </summary>
    [DataField]
    public bool RejuvenateOnShutdown = true;

    /// <summary>
    /// Total HP of a boss. Gets set to Dead MobThreshold when megafauna initializes.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public FixedPoint2 BaseTotalHp = 1;

    [DataField, AutoNetworkedField]
    public float MinAttackCooldown = 0.099f;

    [DataField, AutoNetworkedField]
    public float MaxAttackCooldown = 5f;

    /// <summary>
    /// Defines delay for the first megafauna's attack.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float StartingCooldown = 0.3f;
}
