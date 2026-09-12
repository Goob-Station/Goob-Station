using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Bloodsuckers.Components.Actions;

/// <summary>
/// Configuration for the brawl action.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BloodsuckerBrawlComponent : Component
{
    /// <summary>
    /// Base brute damage on a punch.
    /// </summary>
    [DataField]
    public float PunchBaseDamage = 20f;

    /// <summary>
    /// Additional damage added per action level.
    /// </summary>
    [DataField]
    public float PunchDamagePerLevel = 1f;

    /// <summary>
    /// Armor penetration on the punch.
    /// </summary>
    [DataField]
    public float PunchArmorPen = 20f;

    /// <summary>
    /// Max knockdown duration on a punch, in seconds.
    /// </summary>
    [DataField]
    public float PunchMaxKnockdown = 5f;

    [DataField]
    public SoundSpecifier PunchSound = new SoundPathSpecifier("/Audio/Weapons/punch4.ogg");

    /// <summary>
    /// Base knockdown on the puller when escaping, in seconds.
    /// </summary>
    [DataField]
    public float GrabEscapeKnockdown = 2f;

    [DataField]
    public SoundSpecifier GrabEscapeSound = new SoundPathSpecifier("/Audio/_Goobstation/Weapons/Baton/woodhit.ogg");

    [DataField]
    public SoundSpecifier RestraintBreakSound = new SoundPathSpecifier("/Audio/Weapons/grille_hit.ogg");

    /// <summary>
    /// Do-after delay for bashing open a locker or door, in seconds.
    /// </summary>
    [DataField]
    public float BashDelay = 2.5f;

    /// <summary>
    /// How long the vampire is stunned after forcing a door open.
    /// </summary>
    [DataField]
    public float DoorBashStun = 1f;

    [DataField]
    public SoundSpecifier BashSound = new SoundPathSpecifier("/Audio/Weapons/grille_hit.ogg");

    [DataField]
    public SoundSpecifier DoorPrySound = new SoundPathSpecifier("/Audio/Machines/airlock_creaking.ogg");

    // VS borgs
    [DataField]
    public float EMPRadius = 0f;

    [DataField]
    public float EMPConsumption = 500f;

    [DataField]
    public float EMPDuration = 30f;

    // Level gates
    /// <summary>
    /// Minimum level required to bash lockers.
    /// </summary>
    [DataField]
    public int LockerLevel = 3;

    /// <summary>
    /// Minimum level required to force doors.
    /// </summary>
    [DataField]
    public int DoorLevel = 4;

    /// <summary>
    /// At this level and above, breaking restraints AND escaping a grab
    /// can happen in the same activation.
    /// </summary>
    [DataField]
    public int CombineLevel = 3;

    #region Generic

    /// <summary>
    /// The current level of this action.
    /// </summary>
    public int ActionLevel = 1;

    /// <summary>
    /// The highest level this action can become.
    /// </summary>
    public int MaxLevel = 5;

    /// <summary>
    /// Blood cost deducted from the vampire's bloodstream to activate.
    /// </summary>
    [DataField]
    public float BloodCost = 0f;

    /// <summary>
    /// Humanity lost when activating this action.
    /// </summary>
    [DataField]
    public float HumanityCost = 0f;

    /// <summary>
    /// If true, this action cannot be started while the vampire is in frenzy.
    /// </summary>
    [DataField]
    public bool DisabledInFrenzy = false;

    #endregion
}
