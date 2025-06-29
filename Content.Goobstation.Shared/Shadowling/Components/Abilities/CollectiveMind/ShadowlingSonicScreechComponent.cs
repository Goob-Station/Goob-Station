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
    public float Range = 5f;

    [DataField]
    public TimeSpan SiliconStunTime = TimeSpan.FromSeconds(5);

    [DataField]
    public string WindowTag = "Window";

    [DataField]
    public DamageSpecifier WindowDamage = new()
    {
        DamageDict = new()
        {
            { "Structural", 50 }
        }
    };

    [DataField]
    public float ScreechKick = 80f;

    [DataField]
    public EntProtoId ProtoFlash = "EffectScreech";

    [DataField]
    public SoundSpecifier? ScreechSound = new SoundPathSpecifier("/Audio/_EinsteinEngines/Shadowling/screech.ogg");

    [DataField]
    public EntProtoId SonicScreechEffect = "ShadowlingSonicScreechEffect";
}
