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
    [ViewVariables]
    public Queue<MegafaunaAction> BossAttackQueue = new();

    [ViewVariables]
    public bool Active;

    /// <summary>
    /// Stores name of the last attack that was used by this boss.
    /// </summary>
    [DataField]
    public string? PreviousAttack;

    /// <summary>
    /// How many attacks should we keep in BossAttackSchedule.
    /// </summary>
    [ViewVariables]
    public int AttacksBufferSize = 2;

    /// <summary>
    /// When the boss doesn't die ut for any reason stops attacking,
    /// if this bool is true, will rejuvenate the megafauna.
    /// </summary>
    [DataField]
    public bool RejuvenateOnShutdown = true;

    /// <summary>
    /// Total HP of a boss. Gets set to Dead MobThreshold whe megafauna initializes.
    /// </summary>
    [ViewVariables]
    public FixedPoint2 BaseTotalHp = 1;

    [DataField]
    public float MinAttackCooldown = 0.5f;

    [DataField]
    public float MaxAttackCooldown = 5f;

    [ViewVariables]
    public float NextAttack;
}
