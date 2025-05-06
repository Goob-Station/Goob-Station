using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitcode.Heretic.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class HereticCosmicRuneComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? LinkedRune;

    [DataField]
    public float Range = 0.75f;

    [DataField]
    public SoundSpecifier? Sound = new SoundPathSpecifier("/Audio/_Goobstation/Heretic/cosmic_energy.ogg");

    [DataField]
    public EntProtoId Effect = "HereticRuneCosmosLight";
}
