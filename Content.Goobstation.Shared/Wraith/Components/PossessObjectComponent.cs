using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class PossessObjectComponent : Component
{
    [DataField]
    public SoundSpecifier? Sound;

    [DataField]
    public bool HasMind = false;
}
