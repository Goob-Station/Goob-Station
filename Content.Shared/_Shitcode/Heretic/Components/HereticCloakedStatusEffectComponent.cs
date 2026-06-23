using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._Shitcode.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class HereticCloakedStatusEffectComponent : Component
{
    [DataField]
    public bool RequiresFocus = true;

    [DataField]
    public LocId? LoseFocusMessage;

    [DataField]
    public SoundSpecifier? CloakSound = new SoundCollectionSpecifier("Curse");

    [DataField]
    public SoundSpecifier? UncloakSound = new SoundCollectionSpecifier("Curse");
}
