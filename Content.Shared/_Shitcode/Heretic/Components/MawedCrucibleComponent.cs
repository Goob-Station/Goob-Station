using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Tag;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Shitcode.Heretic.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MawedCrucibleComponent : Component
{
    [DataField]
    public int MaxMass = 3;

    [DataField, AutoNetworkedField]
    public int CurrentMass = 3;

    [DataField]
    public FixedPoint2 EldritchEssencePerMass = 30f;

    [DataField]
    public ProtoId<ReagentPrototype> EldritchEssence = "EldritchEssence";

    [DataField]
    public SoundSpecifier MassGainSound = new SoundPathSpecifier("/Audio/_Goobstation/Heretic/eatfood.ogg");

    [DataField]
    public SoundSpecifier BrewSound = new SoundPathSpecifier("/Audio/Effects/Chemistry/bubbles.ogg");

    [DataField]
    public float Accumulator;

    [DataField]
    public float MassGainTime = 60f;

    [DataField]
    public ProtoId<TagPrototype> EldritchFlaskTag = "EldritchFlask";

    [DataField]
    public ProtoId<TagPrototype> AnchorTag = "AnchorHereticStructure";

    [DataField]
    public EntityWhitelist FuelWhitelist = new()
    {
        Components = new[] { "BodyPart", "Organ" },
    };

    [DataField]
    public List<EntProtoId> Potions = new()
    {
        "PotionCrucibleSoul",
        "PotionDuskDawn",
        "PotionWoundedSoldier",
    };
}

[Serializable, NetSerializable]
public enum CrucibleVisuals : byte
{
    Empty
}
