using Robust.Shared.Audio;

namespace Content.Shared._Goobstation.DumpContainerOnUse;

[RegisterComponent]
public sealed partial class DumpContainerOnUseComponent : Component
{
    [DataField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/Effects/unwrap.ogg");

    [DataField]
    public string ContainerId = "storage";

    [DataField]
    public bool DeleteAfterUse = true;
}
