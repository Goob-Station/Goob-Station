using Content.Shared.Destructible.Thresholds;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

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

    [DataField] public ProtoId<SoundCollectionPrototype> TeleportSounds = "sparks";
}
