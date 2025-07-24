using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._CorvaxNext.MedipenRefiller;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MedipenRefillerComponent : Component
{
    [DataField, AutoNetworkedField]
    public Color CurrentColor = Color.White;

    [DataField, AutoNetworkedField]
    public string CurrentLabel = "";

    [DataField, AutoNetworkedField]
    public int CurrentVolume = 1;

    [DataField, AutoNetworkedField]
    public string PreviewPrototype = "FillableMedipen";

    [DataField]
    public SoundSpecifier ClickSound = new SoundPathSpecifier("/Audio/Machines/machine_switch.ogg");

    [DataField]
    public SoundSpecifier FillMedipenSound = new SoundPathSpecifier("/Audio/Items/hypospray.ogg");

    [DataField]
    public SoundSpecifier RecolorMedipenSound = new SoundPathSpecifier("/Audio/Effects/spray2.ogg");
}
