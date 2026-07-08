using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Slasher.Components;

/// <summary>
/// A charm applied when the Idol damages someone or steals their soul.
/// once fallen they deal less damage to the idol, and say random lines while near them.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class LovestruckComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? Idol;

    [DataField]
    public EntProtoId LovestruckStatusEffect = "StatusEffectSlasherLovestruck";

    [DataField]
    public TimeSpan LovestruckDuration = TimeSpan.FromMinutes(4);

    /// <summary>
    /// Total damage taken from the idol so far.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Accumulated;

    /// <summary>
    /// Damage from the idol required to be charmed.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float ConversionThreshold = 70f;

    /// <summary>
    /// How close the idol has to be for the victim to cheer.
    /// </summary>
    [DataField]
    public float CheerRange = 4f;

    /// <summary>
    /// Random delay between cheers while the idol stays near.
    /// </summary>
    [DataField]
    public TimeSpan MinCheerInterval = TimeSpan.FromSeconds(5);

    [DataField]
    public TimeSpan MaxCheerInterval = TimeSpan.FromSeconds(12);

    /// <summary>
    /// How long the victim is slowed after cheering.
    /// </summary>
    [DataField]
    public float CheerDuration = 1.75f;

    [DataField, AutoNetworkedField]
    public TimeSpan NextCheer;

    /// <summary>
    /// Damage loss when the charmed victim attacks their idol. 0.7 = 70% less damage.
    /// </summary>
    [DataField]
    public float IdolDamageReduction = 0.7f;

    /// <summary>
    /// Cooldown between apologies blurted out for hurting their idol.
    /// </summary>
    [DataField]
    public TimeSpan ApologyCooldown = TimeSpan.FromSeconds(7);

    [DataField, AutoNetworkedField]
    public TimeSpan NextApology;

    /// <summary>
    /// Lines the victim blurts out when they hurt their idol.
    /// </summary>
    [DataField]
    public List<LocId> ApologyLines = new()
    {
        "slasher-lovestruck-apology-1",
        "slasher-lovestruck-apology-2",
        "slasher-lovestruck-apology-3",
        "slasher-lovestruck-apology-4",
        "slasher-lovestruck-apology-5",
    };

    /// <summary>
    /// Movement status entity used to slow the victim mid-cheer.
    /// </summary>
    [DataField]
    public EntProtoId CharmEffect = "SlasherLovestruckStatusEffect";

    /// <summary>
    /// Lines the victim can shout for their idol.
    /// </summary>
    [DataField]
    public List<LocId> CheerLines = new()
    {
        "slasher-lovestruck-cheer-1",
        "slasher-lovestruck-cheer-2",
        "slasher-lovestruck-cheer-3",
        "slasher-lovestruck-cheer-4",
        "slasher-lovestruck-cheer-5",
        "slasher-lovestruck-cheer-6",
        "slasher-lovestruck-cheer-7",
        "slasher-lovestruck-cheer-8",
        "slasher-lovestruck-cheer-9",
        "slasher-lovestruck-cheer-10",
        "slasher-lovestruck-cheer-11",
        "slasher-lovestruck-cheer-12",
        "slasher-lovestruck-cheer-13",
        "slasher-lovestruck-cheer-14",
        "slasher-lovestruck-cheer-15",
    };
}
