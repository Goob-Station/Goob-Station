// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;
using Content.Server._Lavaland.Megafauna.Systems;

namespace Content.Server._Lavaland.Megafauna.Components;

/// <summary>
/// Universal component for megafauna bosses.
/// Use <see cref="AggressiveMegafaunaAiComponent"/>, <see cref="PhasesMegafaunaAiComponent"/>
/// and <see cref="MegafaunaSystem"/> to add actual behavior.
/// </summary>
[RegisterComponent, Access(typeof(MegafaunaSystem))]
public sealed partial class MegafaunaAiComponent : Component
{
    /// <summary>
    /// List of all available megafauna attacks and their conditions to be executed.
    /// </summary>
    [DataField]
    public List<MegafaunaActionEntry> ActionsData = new();

    [ViewVariables]
    public bool Active;

    /// <summary>
    /// Target that is picked before each new attack
    /// </summary>
    [ViewVariables]
    public EntityUid? CurrentTarget;

    [ViewVariables]
    public EntityUid? PreviousTarget;

    /// <summary>
    /// Stores name of the last attack that was used by this boss.
    /// </summary>
    [DataField]
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
    [ViewVariables]
    public FixedPoint2 BaseTotalHp = 1;

    [DataField]
    public float MinAttackCooldown = 0.5f;

    [DataField]
    public float MaxAttackCooldown = 5f;

    [ViewVariables]
    public float NextAttackAccumulator;
}

/// <summary>
/// Contains a list of Conditions before picking this action, and the action itself.
/// By using conditions, you can make a boss actually pick useful attacks in the specific scenarios.
/// TODO: this makes prototypes basically unreadable, need to find simpler ways to write it in YAML
/// </summary>
[DataDefinition]
public partial struct MegafaunaActionEntry
{
    [DataField(required: true)]
    public MegafaunaAction Action;

    [DataField]
    public List<MegafaunaCondition> Conditions = new();
}
