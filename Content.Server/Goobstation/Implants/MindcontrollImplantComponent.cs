

namespace Content.Server.Implants.Components;

[RegisterComponent]
public sealed partial class MindcontrollImplantComponent : Component
{
    [DataField] public EntityUid? HolderUid = null; //who holds the implanter
    [DataField] public EntityUid? ImplanterUid = null; // the implanter carrying the implant
}
