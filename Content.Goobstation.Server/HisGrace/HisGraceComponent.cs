using System.ComponentModel;
using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio;

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
    [ViewVariables(VVAccess.ReadWrite)] public int Hunger;

    /// <summary>
    /// When the next hunger tick is.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan NextHungerTick;

    /// <summary>
    /// The delay between each action tick.
    /// </summary>
    [DataField]
    public TimeSpan TickDelay = TimeSpan.FromSeconds(3);

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
    public int HungerIncrement = 1;

    /// <summary>
    /// How much the hunger is restored by when devouring an entitiy.
    /// </summary>
    [DataField]
    public int HungerOnDevour = 20;

    /// <summary>
    /// How much the damage is currently increased by.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public DamageSpecifier CurrentDamageIncrease = new() { DamageDict = new Dictionary<string, FixedPoint2> { { "Blunt", 0 } } };

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
    /// A dictionary mapping states to the threshold required to get to them, and what their hunrer increment is.
    /// </summary>
    [DataField]
    public Dictionary<HisGraceState, (int Threshold, int Increment)> StateThresholds = new()
    {
        { HisGraceState.Peckish, (0, 1) },

        { HisGraceState.Hungry, (50, 2) },

        { HisGraceState.Ravenous, (100, 3) },

        { HisGraceState.Starving, (150, 4) },

        { HisGraceState.Death, (200, 5) },
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

    /// <summary>
    /// Is His Grace currently being held?
    /// </summary>
    public bool IsHeld;

    /// <summary>
    /// Who is holding His Grace
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public EntityUid? Holder;

    /// <summary>
    /// The time at which His Grace will attack a nearby target.
    /// </summary>
    public TimeSpan NextGroundAttack;

    /// <summary>
    /// The time at which His Grace will attempt to attack his user for being too far.
    /// </summary>
    public TimeSpan NextUserAttack;

    /// <summary>
    /// Sound played on devour
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public SoundSpecifier? SoundDevour = new SoundPathSpecifier("/Audio/Effects/demon_consume.ogg")
    {
        Params = AudioParams.Default.WithVolume(-3f),
    };
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
