using Content.Shared.Body.Part;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Autosurgeon;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AutoSurgeonMultipleComponent : Component
{
    [DataField]
    public List<AutoSurgeonEntry> Entries = new();

    [DataField]
    public bool OneTimeUse = true;

    [DataField, AutoNetworkedField]
    public bool Used;

    [DataField]
    public TimeSpan DoAfterTime = TimeSpan.FromSeconds(15);

    [DataField] // If you're changing this, do not forget the loop
    public SoundSpecifier? Sound = new SoundPathSpecifier("/Audio/_Goobstation/Machines/autosurgeon.ogg", AudioParams.Default.WithLoop(true));

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? ActiveSound;
}

[DataDefinition]
public sealed partial class AutoSurgeonEntry
{
    /// <summary>
    /// Body part to be upgraded. Specify the parent for replacing body part or organ.
    /// </summary>
    [DataField]
    public BodyPartType TargetBodyPart = BodyPartType.Other;

    /// <summary>
    /// Symmetry of the body part to be upgraded. Specify the parent for replacing body part or organ.
    /// </summary>
    [DataField]
    public BodyPartSymmetry TargetBodyPartSymmetry = BodyPartSymmetry.None;

    /// <summary>
    /// If it's null, will upgrade/replace the body part. If not, will search for this organ to upgrade.
    /// For replacing organs, you can write whatever you want here, it just shouldn't be null.
    /// </summary>
    [DataField]
    public string? TargetOrgan;

    /// <summary>
    /// These components will be added to the part itself, for example, MantisBladeArm.
    /// If this is not null but the targeted part has all of them already, the surgery would fail.
    /// This is only applied if NewPartProto is null, so we are upgrading and not replacing a part.
    /// </summary>
    [DataField]
    public ComponentRegistry? ComponentsToPart;

    /// <summary>
    /// These components will be added to organ as BodyPart/Organ.OnAdd and applied to the user, for example, LeftMantisBladeUser.
    /// This is only applied if NewPartProto is null, so we are upgrading and not replacing a part.
    /// </summary>
    [DataField]
    public ComponentRegistry? ComponentsToUser;

    /// <summary>
    /// If it's null, will upgrade the body part/organ. If not, will replace it with this proto.
    /// </summary>
    [DataField]
    public string? NewPartProto;
}
