using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Megafauna.Mercury.Components;

/// <summary>
/// Gains passive charge overtime. Charge increases based on radiation level near item.
/// On use, uses current charge level to spawn in lighting strikes.
/// At full charge it doubles the number of lightning spawned.
/// </summary>
[RegisterComponent]
public sealed partial class EtherDrinkerComponent : Component
{
    /// <summary>
    /// Without any radiation it takes this long to charge.
    /// </summary>
    [DataField]
    public TimeSpan BaseRechargeTime = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Rad per second reduces cooldown by this much per second.
    /// </summary>
    [DataField]
    public float RadiationRechargeMultiplier = 8f;

    /// <summary>
    /// Charge consumed per lightning strike.
    /// </summary>
    [DataField]
    public float ChargePerStrike = 5f;

    /// <summary>
    /// Max number of lighting strikes.
    /// </summary>
    [DataField]
    public int MaxStrikes = 15;

    /// <summary>
    /// Range to spawn strikes relative to the item.
    /// </summary>
    [DataField]
    public int StrikeOffset = 4;

    [DataField]
    public EntProtoId LightningPrototype = "LightningCrackleNeutral";

    [DataField]
    public SoundSpecifier DischargeSound = new SoundPathSpecifier("/Audio/_Lavaland/Mobs/Bosses/Mercury/glitch.ogg");
}

