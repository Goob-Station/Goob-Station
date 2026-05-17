using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Changeling.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ChangelingRegenerateComponent : Component
{
    [DataField]
    public EntProtoId ActionId = "ActionRegenerate";

    [DataField, AutoNetworkedField]
    public EntityUid? ActionEnt;

    [DataField]
    public LocId RegenPopup = "changeling-regenerate";

    [DataField]
    public LocId LimbRegenPopup = "changeling-regenerate-limbs";

    [DataField, AutoNetworkedField]
    public SoundSpecifier RegenSound = new SoundPathSpecifier("/Audio/Effects/demon_consume.ogg")
    {
        Params = AudioParams.Default.WithVolume(-3f),
    };
}
