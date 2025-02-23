namespace Content.Server._Lavaland.Mobs.Hierophant.Components;

[RegisterComponent]
public sealed partial class HierophantBossComponent : MegafaunaComponent
{
    /// <summary>
    /// Amount of time for one damaging tile to charge up and deal the damage to anyone above it.
    /// </summary>
    public const float TileDamageDelay = 0.6f;

    /// <summary>
    ///     Gets calculated automatically in the <see cref="HierophantSystem"/>.
    ///     Is responsive for how fast and strong hierophant attacks.
    /// </summary>
    [ViewVariables]
    public float CurrentAnger = 1f;

    /// <summary>
    /// Minimal amount of anger that Hierophant can have.
    /// Tends to 3 when health tends to 0.
    /// </summary>
    [DataField]
    public float MinAnger = 1f;

    /// <summary>
    /// Max cap for anger.
    /// </summary>
    [DataField]
    public float MaxAnger = 3f;

    [DataField]
    public float InterActionDelay = 3 * TileDamageDelay * 1000f;

    [DataField]
    public float AttackCooldown = 6f * TileDamageDelay;

    [ViewVariables]
    public float AttackTimer = 4f * TileDamageDelay;

    [DataField]
    public float MinAttackCooldown = 2f * TileDamageDelay;

    /// <summary>
    /// Amount of anger to adjust on a hit.
    /// </summary>
    [DataField]
    public float AdjustAngerOnAttack = 0.1f;

    /// <summary>
    /// Connected field generator, will try to teleport here when it's inactive.
    /// </summary>
    [ViewVariables]
    public EntityUid? ConnectedFieldGenerator;

    /// <summary>
    /// Controls
    /// </summary>
    [DataField]
    public Dictionary<HierophantAttackType, float> Attacks = new()
    {
        { HierophantAttackType.Chasers, 0.1f },
        { HierophantAttackType.Crosses, 0.1f },
        { HierophantAttackType.DamageArea, 0.2f },
        { HierophantAttackType.Blink, 0.2f },
    };

    /// <summary>
    /// Attack that was done previously, so we don't repeat it over and over.
    /// </summary>
    [DataField]
    public HierophantAttackType PreviousAttack;
}

public enum HierophantAttackType
{
    Invalid,
    Chasers,
    Crosses,
    DamageArea,
    Blink,
}
