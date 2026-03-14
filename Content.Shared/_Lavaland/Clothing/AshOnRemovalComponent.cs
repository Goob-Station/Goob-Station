using Robust.Shared.Audio;

namespace Content.Shared._Lavaland.Clothing;

/// <summary>
/// This is used for clothing that ash the wearer when removed
/// </summary>
[RegisterComponent]
public sealed partial class AshOnRemovalComponent : Component
{
    /// <summary>
    ///  if this is disabled
    /// </summary>
    [DataField]
    public bool Enabled = true;

    /// <summary>
    /// This gets played when user is cremated This is predicted by the client.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField]
    public SoundSpecifier Sound { get; set; } = new SoundPathSpecifier("/Audio/Effects/lightburn.ogg")
    {
        Params = AudioParams.Default.WithVolume(3f).WithVariation(0.025f),
    };
}
