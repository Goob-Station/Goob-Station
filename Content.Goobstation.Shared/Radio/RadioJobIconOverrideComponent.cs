using Content.Shared.StatusIcon;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Radio;

[RegisterComponent, Access(typeof(RadioJobIconSystem))]
public sealed partial class RadioJobIconOverrideComponent : Component
{
    [DataField(required: true)]
    public ProtoId<JobIconPrototype> JobIcon;

    [DataField]
    public LocId? JobName;
}
