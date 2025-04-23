using Robust.Shared.Audio;
using Robust.Shared.Map;

namespace Content.Server._Goobstation.DropPod;

[RegisterComponent]
public sealed partial class DropPodGridComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public EntityCoordinates? TargetCoords;

    [DataField]
    public SoundSpecifier? ArriveSound;
}
