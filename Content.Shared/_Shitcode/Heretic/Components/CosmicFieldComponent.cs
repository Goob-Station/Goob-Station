using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._Shitcode.Heretic.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CosmicFieldComponent : Component
{
    [DataField, AutoNetworkedField]
    public int Strength;

    [DataField]
    public SoundSpecifier BombDefuseSound = new SoundPathSpecifier("/Audio/Effects/lightburn.ogg");

    [DataField]
    public LocId BombDefusePopup = "cosmic-field-component-bomb-defused-message";
}
