using Content.Shared.Access;
using Content.Shared.Roles;
using Content.Shared.StatusIcon;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Shitcode.Heretic.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class EldritchIdCardComponent : Component
{
    [DataField, AutoNetworkedField]
    public List<EldritchIdConfiguration> Configs = new();

    [DataField, AutoNetworkedField]
    public EntProtoId? CurrentProto;

    [DataField, AutoNetworkedField]
    public bool Inverted;

    [DataField, AutoNetworkedField]
    public EntityUid? PortalOne;

    [DataField, AutoNetworkedField]
    public EntityUid? PortalTwo;

    [DataField]
    public EntProtoId Portal = "RealityCrack";

    [DataField]
    public SoundSpecifier EatSound = new SoundPathSpecifier("/Audio/_Goobstation/Heretic/eatfood.ogg");
}

[Serializable, NetSerializable]
public sealed class EldritchIdConfiguration(
    string? fullName,
    string? jobTitle,
    ProtoId<JobIconPrototype>  jobIcon,
    List<ProtoId<DepartmentPrototype>> departments,
    HashSet<ProtoId<AccessLevelPrototype>> tags,
    EntProtoId cardPrototype)
{
    [DataField]
    public string? FullName = fullName;

    [DataField]
    public string? JobTitle = jobTitle;

    [DataField]
    public ProtoId<JobIconPrototype> JobIcon = jobIcon;

    [DataField]
    public HashSet<ProtoId<AccessLevelPrototype>> AccessTags = tags;

    [DataField]
    public List<ProtoId<DepartmentPrototype>> Departments = departments;

    [DataField]
    public EntProtoId CardPrototype = cardPrototype;
}
