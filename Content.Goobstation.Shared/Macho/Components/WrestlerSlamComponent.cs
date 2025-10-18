using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wrestler.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class WrestlerSlamComponent : Component
{
    [DataField]
    public TimeSpan StunDuration = TimeSpan.FromSeconds(2);

    [DataField]
    public SoundSpecifier? Sound = new SoundCollectionSpecifier("MachoRage");
}
