using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Throwing;

[RegisterComponent, NetworkedComponent]
public sealed partial class SwapTeleportOnThrowComponent : Component
{
    /// <summary>
    /// Sound that is played on a new position that thrower just teleported into.
    /// </summary>
    [DataField]
    public SoundSpecifier? TargetSound = new SoundPathSpecifier("/Audio/Effects/teleport_departure.ogg");

    /// <summary>
    /// Sound that is played on a previous position of a thrower.
    /// </summary>
    [DataField]
    public SoundSpecifier? OriginSound = new SoundPathSpecifier("/Audio/Effects/teleport_arrival.ogg");
}
