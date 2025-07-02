// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.HisGrace;

[RegisterComponent, NetworkedComponent]
public sealed partial class HisGraceComponent : Component
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

    public int Hunger
    {
        get => _hunger;
        set => _hunger = Math.Max(value, 0);
    }

    /// <summary>
    /// The current hunger of His Grace.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    private int _hunger;

    /// <summary>
    /// When the next hunger tick is.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan NextTick;

    /// <summary>
    /// The delay between each action tick.
    /// </summary>
    [DataField]
    public TimeSpan TickDelay = TimeSpan.FromSeconds(3);

    /// <summary>
    /// How much the hunger decreases per tick.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public int HungerIncrement = 1;

    /// <summary>
    /// The hunger given by an entity is their critical state threshold times this number.
    /// </summary>
    [DataField]
    public float HungerOnDevourMultiplier = 0.2f;

    /// <summary>
    /// The default hunger given, if no state can be found.
    /// </summary>
    [DataField]
    public int HungerOnDevourDefault = 5;

    /// <summary>
    /// This amount of speed is added to the target for every level of hunger they gain.
    /// The hungrier, the faster.
    /// </summary>
    [DataField]
    public float SpeedAddition = 0.2f;

    /// <summary>
    /// How much the speed addition will be multiplied for each subsequent level?
    /// E.G : 0.2 - 0.4 - 0.8
    /// </summary>
    [DataField]
    public float SpeedIncrementMultiplier = 2f;

    /// <summary>
    /// How many entities do you need to consume to ascend?
    /// </summary>
    [DataField]
    public int AscensionThreshold = 25;

    /// <summary>
    /// How much the damage is currently increased by.
    /// </summary>
    /// <remarks>
    /// This starts defined as zero so we can increase it.
    /// </remarks>
    [ViewVariables(VVAccess.ReadWrite)]
    public DamageSpecifier CurrentDamageIncrease = new() // evil? yes. ugly? less so.
    {
        DamageDict =
        {
            ["Blunt"] = 0,
        },
    };

    /// <summary>
    /// The base damage of the item.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public DamageSpecifier BaseDamage = new();

    /// <summary>
    /// How much His Grace heals you per tick.
    /// </summary>
    [DataField]
    public DamageSpecifier Healing = new();

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
    public DamageSpecifier DamageOnFail = new();

    /// <summary>
    /// Where the entities go when it devours them, empties on user death.
    /// </summary>
    public Container Stomach = new();

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
    /// Sound played on devour
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public readonly SoundSpecifier? SoundDevour = new SoundPathSpecifier("/Audio/Effects/demon_consume.ogg")
    {
        Params = AudioParams.Default.WithVolume(-3f),
    };

    /// <summary>
    /// The states ordered in ascending order.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public List<KeyValuePair<HisGraceState,(int Threshold, int Increment)>> OrderedStates = [];

    /// <summary>
    /// The damage coeficcient the user uses when ascended.
    /// </summary>
    [DataField]
    public float AscensionDamageCoefficient = 0.4f;

    /// <summary>
    /// The damage coeficcient the user uses when not ascended.
    /// </summary>
    [DataField]
    public float DefaultDamageCoefficient = 0.7f;

    [ViewVariables]
    public float BaseStamCritThreshold;

    /// <summary>
    /// The stam crit threshold the user gains when holding.
    /// </summary>
    [DataField]
    public float HoldingStamCritThreshold = 1000f;

    [DataField]
    public SoundSpecifier AscendSound = new SoundPathSpecifier("/Audio/_Goobstation/Ambience/Antag/hisgrace_ascension.ogg")
    {
        Params = AudioParams.Default.WithVolume(2f),
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
