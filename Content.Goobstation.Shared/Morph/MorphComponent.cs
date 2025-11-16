using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Alert;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Morph;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class MorphComponent : Component
{
    /// <summary>
    /// the amount of biomass a morph has
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public FixedPoint2 Biomass;

    /// <summary>
    /// How much it costs to replicate
    /// </summary>
    [DataField]
    public float ReplicateCost = 60;

    /// <summary>
    /// How much biomass one Morph can store
    /// </summary>
    [DataField]
    public float MaxBiomass = 120;

    /// <summary>
    /// How long it takes to replicate, 5f = 5 seconds
    /// </summary>
    [DataField]
    public float ReplicationDelay = 5f;

    /// <summary>
    /// Amount of morphs this morph has produced, used for end round text
    /// </summary>
    [DataField]
    public static int Children = 0;

    /// <summary>
    /// What mob to spawn on replicate, could be used for some sort of sac to spawn morphs on, just uses morph prototype for now
    /// </summary>
    [DataField]
    public string MorphPrototype = "MobMorph";

    /// <summary>
    /// what damage the morph needs to take in order to revert the disguise
    /// </summary>
    [DataField]
    public FixedPoint2 DamageThreshold = FixedPoint2.New(2);

    /// <summary>
    /// The alert to add on MapInit
    /// </summary>
    [DataField]
    public ProtoId<AlertPrototype> BiomassAlert = "Biomass";

    /// <summary>
    /// Controls Digits for morph
    /// </summary>
    [NetSerializable, Serializable]
    public enum MorphVisualLayers : byte
    {
        Digit1,
        Digit2,
        Digit3
    }

    // Morph Actions
    [DataField]
    public string MorphReplicate = "ActionMorphReplicate";

    [DataField]
    public string Morph = "ActionMorph";

    [DataField]
    public string UnMorph = "ActionUnMorph";

    // Morph Sounds
    [DataField]
    public SoundSpecifier ReplicateSound = new SoundPathSpecifier("/Audio/_Harmony/Misc/mutate.ogg");

    /// <summary>
    /// If non-null, whitelist for valid entities that provide biomass.
    /// </summary>
    [DataField(required: true)]
    public EntityWhitelist? BiomassWhitelist;

    /// <summary>
    /// If non-null, blacklist that does not provide biomass.
    /// </summary>
    [DataField(required: true)]
    public EntityWhitelist? BiomassBlacklist;
}
