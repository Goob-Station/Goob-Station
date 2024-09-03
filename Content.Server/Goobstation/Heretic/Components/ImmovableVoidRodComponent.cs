using Robust.Shared.Audio;

namespace Content.Server.Heretic.Components;

[RegisterComponent]
public sealed partial class ImmovableVoidRodComponent : Component
{
    [DataField] public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/Effects/bang.ogg");

    [DataField] public TimeSpan Lifetime = TimeSpan.FromSeconds(1f);
    public float Accumulator = 0f;
}
