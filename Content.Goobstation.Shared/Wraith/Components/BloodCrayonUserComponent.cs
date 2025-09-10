using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Wraith.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class BloodWritingComponent : Component
{
    [DataField]
    public EntityUid? BloodWriting;
}
