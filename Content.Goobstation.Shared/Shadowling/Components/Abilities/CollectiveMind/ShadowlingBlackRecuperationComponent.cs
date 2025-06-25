using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Polymorph;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;


namespace Content.Shared._EE.Shadowling;


/// <summary>
/// This is used for the Black Recuperation ability.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingBlackRecuperationComponent : Component
{
    [DataField]
    public bool IsEmpowering;

    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(5);

    [DataField]
    public string? BlackRecuperationEffect = "ShadowlingBlackRecuperationEffect";

    [DataField]
    public int LesserShadowlingMaxLimit = 5;

    [DataField]
    public int LesserShadowlingAmount;

    [DataField]
    public ProtoId<PolymorphPrototype> LesserShadowlingSpeciesProto = "ShadowPolymorph";

    [DataField]
    public string LesserShadowlingEyes = "LesserShadowlingEyes";

    [DataField]
    public SoundSpecifier? BlackRecSound = new SoundPathSpecifier("/Audio/Items/Defib/defib_zap.ogg");

    [DataField]
    public DamageSpecifier DamageToDeal = new()
    {
        DamageDict = new()
        {
            ["Cellular"] = 5,
        }
    };

    [DataField]
    public float ResistanceRemoveFromThralls = 0.5f;

    [DataField]
    public float ResistanceRemoveFromLesser = 0.12f;
}

