// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._Lavaland.Megafauna.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Megafauna;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MegafaunaAiComponent : Component
{
    /// <summary>
    /// List of all available megafauna attacks and their conditions to be executed.
    /// </summary>
    [DataField(required: true), ViewVariables(VVAccess.ReadOnly)]
    public ProtoId<MegafaunaPatternsListPrototype> ActionsDataId;

    [ViewVariables, AutoNetworkedField]
    public bool Active;

    [ViewVariables, AutoNetworkedField]
    public Queue<MegafaunaAction> ActionQueue;

    [ViewVariables, AutoNetworkedField]
    public TimeSpan NextAction;

    [DataField, AutoNetworkedField]
    public int ActionQueueBufferSize = 3;

    /// <summary>
    /// Target that is picked before each new attack
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public EntityUid? CurrentTarget;

    [ViewVariables, AutoNetworkedField]
    public EntityUid? PreviousTarget;

    /// <summary>
    /// Stores name of the last attack that was used by this boss.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? PreviousAttack;

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
    public float MinAttackCooldown = 0.5f;

    [DataField, AutoNetworkedField]
    public float MaxAttackCooldown = 5f;
}
