using Content.Shared.Destructible.Thresholds;
using Robust.Shared.Audio;

namespace Content.Server.Teleportation;

/// <summary>
///     Component to store parameters for entities that teleport randomly.
/// </summary>
[RegisterComponent, Virtual]
public partial class RandomTeleportComponent : Component
{
    /// <summary>
    ///     Up to how far to teleport the user in tiles.
    /// </summary>
    [DataField] public MinMax Radius = new MinMax(10, 20);

    /// <summary>
    ///     How many times to try to pick the destination. Larger number means the teleport is more likely to be safe.
    /// </summary>
    [DataField] public int TeleportAttempts = 10;

    /// <summary>
    ///     Will try harder to find a safe teleport.
    /// </summary>
    [DataField] public bool ForceSafeTeleport = true;

    [DataField] public SoundSpecifier ArrivalSound = new SoundPathSpecifier("/Audio/Effects/teleport_arrival.ogg");
    [DataField] public SoundSpecifier DepartureSound = new SoundPathSpecifier("/Audio/Effects/teleport_departure.ogg");
}
