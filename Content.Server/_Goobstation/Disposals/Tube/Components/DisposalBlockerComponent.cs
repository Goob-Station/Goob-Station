using Content.Server.Disposal.Tube;
using Content.Server.Disposal.Tube.Components;

namespace Content.Server._Goobstation.Disposals.Tube.Components;

[RegisterComponent]
[Access(typeof(DisposalTubeSystem))]
public sealed partial class DisposalBlockerComponent : DisposalTransitComponent
{
}
