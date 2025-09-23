using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.BloodCult.Components;

[NetworkedComponent, RegisterComponent]
public sealed partial class PentagramComponent : Component
{
    public ResPath RsiPath = new("/Textures/WhiteDream/BloodCult/Effects/pentagram.rsi");

    public readonly string[] States =
    [
        "halo1",
        "halo2",
        "halo3",
        "halo4",
        "halo5",
        "halo6"
    ];
}
