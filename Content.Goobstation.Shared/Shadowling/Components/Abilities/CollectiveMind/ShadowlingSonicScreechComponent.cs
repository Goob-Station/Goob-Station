using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Shadowling.Components.Abilities.CollectiveMind;

/// <summary>
/// This is used for the Sonic Screech ability.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingSonicScreechComponent : Component
{
    [DataField]
    public EntProtoId ActionId = "ActionSonicScreech";

    [DataField]
    public EntityUid? ActionEnt;

    /// <summary>
    /// The search radius of the ability.
    /// </summary>
    [DataField]
    public float Range = 5f;

    /// <summary>
    /// The amount of time silicons get stunned for (IPCs currently)
    /// </summary>
    [DataField]
    public TimeSpan SiliconStunTime = TimeSpan.FromSeconds(5);

    /// <summary>
    /// The tag that indicates that the obstacle hit by the ability is a window.
    /// </summary>
    [DataField]
    public string WindowTag = "Window";

    /// <summary>
    /// How much damage the window structures take from this ability.
    /// </summary>
    [DataField]
    public DamageSpecifier WindowDamage = new()
    {
        DamageDict = new()
        {
            { "Structural", 50 }
        }
    };

    /// <summary>
    /// The prototype of the flash that gets thrown on the targets of this ability.
    /// </summary>
    [DataField]
    public EntProtoId ProtoFlash = "EffectScreech";

    /// <summary>
    /// The sound that plays once the ability is used.
    /// </summary>
    [DataField]
    public SoundSpecifier? ScreechSound = new SoundPathSpecifier("/Audio/_EinsteinEngines/Shadowling/screech.ogg");

    /// <summary>
    /// The effect that is used once the ability activates.
    /// </summary>
    [DataField]
    public EntProtoId SonicScreechEffect = "ShadowlingSonicScreechEffect";
}
