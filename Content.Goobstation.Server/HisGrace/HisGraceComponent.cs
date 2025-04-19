using System.ComponentModel;
using Content.Shared.Damage;
using Content.Shared.FixedPoint;

namespace Content.Goobstation.Server.HisGrace;

[RegisterComponent]
public sealed partial class HisGraceComponent : Robust.Shared.GameObjects.Component
{
    /// <summary>
    /// The Entity bound to His Grace
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public EntityUid? User;

    /// <summary>
    /// Is His Grace currently activated?
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public bool IsActivated;

    /// <summary>
    /// The current state of His Grace.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public HisGraceState CurrentState = HisGraceState.Dormant;

    /// <summary>
    /// How many entities has His Grace consumed?
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public int EntitiesAbsorbed;

    /// <summary>
    /// The current hunger of his grace.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public int Hunger = Math.Clamp(0, 0, 200);

    /// <summary>
    /// When the next hunger tick is.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan NextHungerTick;

    /// <summary>
    /// The delay between each hunger tick.
    /// </summary>
    [DataField]
    public TimeSpan HungerTickDelay = TimeSpan.FromSeconds(3);

    /// <summary>
    /// State to hunger increment dict
    /// </summary>
    [DataField]
    public Dictionary<HisGraceState, int> HungerIncrementThresholds = new()
    {
        { HisGraceState.Dormant, 1 },

        { HisGraceState.Peckish, 1 },

        { HisGraceState.Hungry, 2 },

        { HisGraceState.Ravenous, 3 },

        { HisGraceState.Starving, 4 },

        { HisGraceState.Ascended, 0 },
    };

    /// <summary>
    /// How much the hunger decreases per tick.
    /// </summary>
    [DataField]
    public int HungerIncrement;

    /// <summary>
    /// How much the hunger is restored by when devouring an entitiy.
    /// </summary>
    [DataField]
    public int HungerOnDevour = 20;

    /// <summary>
    /// How much the damage is currently increased by.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public FixedPoint2 CurrentDamageIncrease;

    /// <summary>
    /// The base damage of the item.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public DamageSpecifier BaseDamage;

    /// <summary>
    /// How much His Grace heals you per tick.
    /// </summary>
    [DataField]
    public DamageSpecifier Healing;

    /// <summary>
    /// The current hunger of his grace.
    /// </summary>
    [DataField]
    public Dictionary<HisGraceState, int> StateThreshholds = new()
    {
        { HisGraceState.Peckish, 0 },

        { HisGraceState.Hungry, 50 },

        { HisGraceState.Ravenous, 100 },

        { HisGraceState.Starving, 150 },

        { HisGraceState.Death, 200 },
    };

    /// <summary>
    /// The damage dealt to an entity when it fails to feed His Grace.
    /// </summary>
    [DataField]
    public DamageSpecifier DamageOnFail = new() { DamageDict = new Dictionary<string, FixedPoint2> { { "Slash", 300 } } };

    /// <summary>
    /// Where the entities go when it devours them, empties on user death.
    /// </summary>
    public Robust.Shared.Containers.Container Stomach;

    public bool IsHeld;

    public bool CanAttack = true;
}

public enum HisGraceState : byte
{
    Dormant,
    Peckish,
    Hungry,
    Ravenous,
    Starving,
    Death,
    Ascended,
}
