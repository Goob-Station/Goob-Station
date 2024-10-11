using Robust.Shared.Audio;

namespace Content.Server.Teleportation;

/// <summary>
/// Component to store parameters for entities that teleport randomly.
/// </summary>
[RegisterComponent]
public sealed partial class RandomTeleportComponent : Component
{
    /// <summary>
    /// Up to how far to teleport the user
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float TeleportRadius = 100f;

    /// <summary>
    /// How many times to check for a valid tile to teleport to, higher number means less teleports into walls or open space
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int TeleportAttempts = 20;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public SoundSpecifier TeleportSound = new SoundPathSpecifier("/Audio/Effects/teleport_arrival.ogg");
}
