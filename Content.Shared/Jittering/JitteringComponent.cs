using System.Numerics;
using Robust.Shared.GameStates;

namespace Content.Shared.Jittering;

[Access(typeof(SharedJitteringSystem))]
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class JitteringComponent : Component
{
    [AutoNetworkedField]
    [ViewVariables(VVAccess.ReadWrite)]
    public float Amplitude { get; set; } = 10f; // Goob edit

    [AutoNetworkedField]
    [ViewVariables(VVAccess.ReadWrite)]
    public float Frequency { get; set; } = 4f; // Goob edit

    [ViewVariables(VVAccess.ReadWrite)]
    public Vector2 LastJitter { get; set; }

    /// <summary>
    ///     The offset that an entity had before jittering started,
    ///     so that we can reset it properly.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public Vector2 StartOffset = Vector2.Zero;
}
