using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

[RegisterComponent, NetworkedComponent]
public sealed partial class CursedDeathComponent : Component
{
    /// <summary>
    /// Damage to be dealt by curse.
    /// </summary>
    [DataField]
    public DamageSpecifier DamageCurse = new()
    {
        DamageDict = new()
        {
            { "Blunt", 2},
            { "Slash", 2 },
            { "Piercing", 2 },
            { "Heat", 2},
            { "Cold", 2},
            { "Asphyxiation", 2 }
        }
    };

    /// <summary>
    /// How long before they take damage.
    /// </summary>
    [DataField]
    public TimeSpan TimeTillDamage = TimeSpan.FromSeconds(10);

    /// <summary>
    /// How long before they stun.
    /// </summary>
    [DataField]
    public TimeSpan TimeTillStun = TimeSpan.FromSeconds(10);

    /// <summary>
    /// How long before they take stamina damage.
    /// </summary>
    [DataField]
    public TimeSpan TimeTillStaminaDamage = TimeSpan.FromSeconds(15);

    /// <summary>
    /// How long before they gib.
    /// </summary>
    [DataField]
    public TimeSpan TimeTillGib = TimeSpan.FromSeconds(70);

    /// <summary>
    /// How long they are stunned for.
    /// </summary>
    [DataField]
    public TimeSpan StunDuration = TimeSpan.FromSeconds(3);

    /// <summary>
    /// How much stamina damage to apply over time.
    /// </summary>
    [DataField]
    public float StaminaDamageAmount;

    /// <summary>
    /// How much stamina damage to apply over time.
    /// </summary>
    [DataField]
    public float StaminaDamageIncrease = 50f;

    /// <summary>
    /// How much blood they lose per tick.
    /// </summary>
    [DataField]
    public float BloodToSpill = 30f;

    /// <summary>
    /// How long before they puke blood.
    /// </summary>
    [DataField]
    public TimeSpan TimeTillGore = TimeSpan.FromSeconds(25);

    /// <summary>
    /// Next time at which they will puke blood.
    /// </summary>
    public TimeSpan NextTickGore = TimeSpan.Zero;

    /// <summary>
    /// Next time at which they will puke a lot of blood.
    /// </summary>
    public TimeSpan NextTickDamage = TimeSpan.Zero;

    /// <summary>
    /// Next time at which they will puke a lot of blood.
    /// </summary>
    public TimeSpan NextTickStun = TimeSpan.Zero;

    /// <summary>
    /// Next time at which they will puke a lot of blood.
    /// </summary>
    public TimeSpan NextTickStaminaDamage = TimeSpan.Zero;

    /// <summary>
    /// Next time at which they will gib.
    /// </summary>
    public TimeSpan NextTickGib = TimeSpan.Zero;

    [DataField]
    public EntProtoId SmokeProto = "AdminInstantEffectSmoke10";

    /// <summary>
    /// Next line for popup to say.
    /// </summary>
    [DataField]
    public int NextLine;

    /// <summary>
    /// How long before the next popup.
    /// </summary>
    [DataField]
    public TimeSpan TimeTillPopup = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Next time at which they will see a message.
    /// </summary>
    public TimeSpan NextTickPopup = TimeSpan.Zero;

    [DataField]
    public SoundSpecifier? CurseSound1 = new SoundPathSpecifier("/Audio/_Goobstation/Wraith/Ambience/Void_Wail.ogg");

    [DataField]
    public SoundSpecifier? CurseSound2 = new SoundPathSpecifier("/Audio/_Goobstation/Wraith/wraithwhisper3.ogg");

    [DataField]
    public SoundSpecifier? CurseSound3 = new SoundPathSpecifier("/Audio/_Goobstation/Wraith/wraithwhisper2.ogg");
}
