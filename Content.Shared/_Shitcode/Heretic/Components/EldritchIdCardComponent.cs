using System.Linq;
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
    public HashSet<EldritchIdConfiguration> Configs = new();

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
    ProtoId<JobIconPrototype> jobIcon,
    List<ProtoId<DepartmentPrototype>> departments,
    HashSet<ProtoId<AccessLevelPrototype>> tags,
    EntProtoId cardPrototype)
{
    public string? FullName = fullName;
    public string? JobTitle = jobTitle;
    public ProtoId<JobIconPrototype> JobIcon = jobIcon;
    public HashSet<ProtoId<AccessLevelPrototype>> AccessTags = tags;
    public List<ProtoId<DepartmentPrototype>> Departments = departments;
    public EntProtoId CardPrototype = cardPrototype;

    public override bool Equals(object? obj)
    {
        if (obj is not EldritchIdConfiguration other)
            return false;

        return FullName == other.FullName &&
               JobTitle == other.JobTitle &&
               JobIcon.Id == other.JobIcon.Id &&
               CardPrototype.Id == other.CardPrototype.Id &&
               AccessTags.SetEquals(other.AccessTags) &&
               Departments.SequenceEqual(other.Departments);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();

        hash.Add(FullName);
        hash.Add(JobTitle);
        hash.Add(JobIcon.Id);
        hash.Add(CardPrototype.Id);

        hash.Add(AccessTags.Count);
        foreach (var tag in AccessTags.OrderBy(x => x.Id))
        {
            hash.Add(tag);
        }

        hash.Add(Departments.Count);
        foreach (var dept in Departments)
        {
            hash.Add(dept);
        }

        return hash.ToHashCode();
    }
}
