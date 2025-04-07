using Content.Shared.Body.Systems;
using Robust.Shared.GameStates;

// Shitmed Change
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;
using Content.Shared._Shitmed.Medical.Surgery;
using Content.Shared._Shitmed.Medical.Surgery.Tools;
using Content.Shared._Shitmed.Medical.Surgery.Traumas;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Systems;
using Robust.Shared.Audio;

namespace Content.Shared.Body.Organ;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedBodySystem), typeof(SharedSurgerySystem), typeof(TraumaSystem))] // Shitmed Change
public sealed partial class OrganComponent : Component, ISurgeryToolComponent // Shitmed Change
{
    /// <summary>
    /// Relevant body this organ is attached to.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Body;

    // Shitmed Change Start
    /// <summary>
    ///     Shitmed Change:Relevant body this organ originally belonged to.
    ///     FOR WHATEVER FUCKING REASON AUTONETWORKING THIS CRASHES GIBTEST AAAAAAAAAAAAAAA
    /// </summary>
    [DataField]
    public EntityUid? OriginalBody;

    /// <summary>
    ///     Maximum organ integrity, do keep in mind that Organs are supposed to be VERY and VERY damage sensitive
    /// </summary>
    [DataField("intCap"), AutoNetworkedField]
    public FixedPoint2 IntegrityCap = 15;

    /// <summary>
    ///     Current organ HP, or integrity, whatever you prefer to say
    /// </summary>
    [DataField("integrity"), AutoNetworkedField]
    public FixedPoint2 OrganIntegrity = 15;

    /// <summary>
    ///     Current Organ severity, dynamically updated based on organ integrity
    /// </summary>
    [DataField, AutoNetworkedField]
    public OrganSeverity OrganSeverity = OrganSeverity.Normal;

    /// <summary>
    ///     Sound played when this organ gets turned into a blood mush.
    /// </summary>
    [DataField]
    public SoundSpecifier OrganDestroyedSound = new SoundCollectionSpecifier("OrganDestroyed");

    /// <summary>
    ///     All the modifiers that are currently modifying the OrganIntegrity
    /// </summary>
    public Dictionary<(string, EntityUid), FixedPoint2> IntegrityModifiers = new();

    /// <summary>
    ///     The name's self-explanatory, thresholds. for states. of integrity. of this god fucking damn organ.
    /// </summary>
    [DataField] //TEMPORARY: MAKE REQUIRED WHEN EVERY YML HAS THESE.
    public Dictionary<OrganSeverity, FixedPoint2> IntegrityThresholds = new()
    {
        { OrganSeverity.Normal, 15 },
        { OrganSeverity.Damaged, 10 },
        { OrganSeverity.Destroyed, 0 },
    };

    /// <summary>
    ///     Shitmed Change: Shitcodey solution to not being able to know what name corresponds to each organ's slot ID
    ///     without referencing the prototype or hardcoding.
    /// </summary>

    [DataField]
    public string SlotId = string.Empty;

    [DataField]
    public string ToolName { get; set; } = "An organ";

    [DataField]
    public float Speed { get; set; } = 1f;

    /// <summary>
    ///     Shitmed Change: If true, the organ will not heal an entity when transplanted into them.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool? Used { get; set; }


    /// <summary>
    ///     When attached, the organ will ensure these components on the entity, and delete them on removal.
    /// </summary>
    [DataField]
    public ComponentRegistry? OnAdd;

    /// <summary>
    ///     When removed, the organ will ensure these components on the entity, and delete them on insertion.
    /// </summary>
    [DataField]
    public ComponentRegistry? OnRemove;

    /// <summary>
    ///     Is this organ working or not?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Enabled = true;

    /// <summary>
    ///     Can this organ be enabled or disabled? Used mostly for prop, damaged or useless organs.
    /// </summary>
    [DataField]
    public bool CanEnable = true;
    // Shitmed Change End
}
