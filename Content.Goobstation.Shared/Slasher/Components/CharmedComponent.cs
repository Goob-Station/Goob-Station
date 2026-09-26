using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Slasher.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CharmedComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? Idol;

    [DataField]
    public EntProtoId CharmedStatusEffect = "StatusEffectSlasherCharmed";

    [DataField]
    public TimeSpan CharmedDuration = TimeSpan.FromMinutes(4);

    /// <summary>
    /// Total damage from the idol.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Accumulated;

    /// <summary>
    /// How much damage it takes to get charmed.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float ConversionThreshold = 70f;

    /// <summary>
    /// How close the Idol has to be for the victim to cheer.
    /// </summary>
    [DataField]
    public float CheerRange = 4f;

    [DataField]
    public TimeSpan MinCheerInterval = TimeSpan.FromSeconds(5);

    [DataField]
    public TimeSpan MaxCheerInterval = TimeSpan.FromSeconds(12);

    /// <summary>
    /// How long the victim is slowed while cheering.
    /// </summary>
    [DataField]
    public TimeSpan CheerDuration = TimeSpan.FromSeconds(1.75);

    [DataField, AutoNetworkedField]
    public TimeSpan NextCheer;

    [DataField]
    public float IdolDamageModifier = 0.7f;

    [DataField]
    public TimeSpan ApologyCooldown = TimeSpan.FromSeconds(7);

    [DataField, AutoNetworkedField]
    public TimeSpan NextApology;

    [DataField]
    public EntProtoId CheerSlowdown = "SlasherCharmedStatusEffect";

    [DataField]
    public List<LocId> ApologyLines = new()
    {
        "slasher-charmed-apology-1",
        "slasher-charmed-apology-2",
        "slasher-charmed-apology-3",
        "slasher-charmed-apology-4",
        "slasher-charmed-apology-5",
    };

    [DataField]
    public List<LocId> CheerLines = new()
    {
        "slasher-charmed-cheer-1",
        "slasher-charmed-cheer-2",
        "slasher-charmed-cheer-3",
        "slasher-charmed-cheer-4",
        "slasher-charmed-cheer-5",
        "slasher-charmed-cheer-6",
        "slasher-charmed-cheer-7",
        "slasher-charmed-cheer-8",
        "slasher-charmed-cheer-9",
        "slasher-charmed-cheer-10",
        "slasher-charmed-cheer-11",
        "slasher-charmed-cheer-12",
        "slasher-charmed-cheer-13",
        "slasher-charmed-cheer-14",
        "slasher-charmed-cheer-15",
    };

    [DataField]
    public Box2 BarBounds = new(new Vector2(0.38f, -0.28f), new Vector2(0.46f, 0.32f));

    [DataField]
    public Vector2 BarBorder = new(0.015f, 0.015f);

    [DataField]
    public Color BarBackgroundColor = new(0.05f, 0.02f, 0.04f, 0.72f);

    [DataField]
    public Color BarEmptyColor = Color.FromHex("#5C1030");

    [DataField]
    public Color BarFullColor = Color.FromHex("#FF5C8A");
}
