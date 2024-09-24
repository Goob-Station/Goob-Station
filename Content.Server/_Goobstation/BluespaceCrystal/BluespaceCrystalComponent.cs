using Robust.Shared.Audio;

namespace Content.Server._Goobstation.BluespaceCrystal;

/// <summary>
/// Randomly teleports entity when triggered.
/// </summary>
[RegisterComponent]
public sealed partial class BluespaceCrystalComponent : Component
{
    /// <summary>
    /// Up to how far to teleport the user
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float TeleportRadius = 150f;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public SoundSpecifier TeleportSound = new SoundPathSpecifier("/Audio/Effects/teleport_arrival.ogg");
}
