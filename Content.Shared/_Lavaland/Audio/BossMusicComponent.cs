using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Audio;

[RegisterComponent, AutoGenerateComponentState]
public sealed partial class BossMusicComponent : Component
{
    [AutoNetworkedField]
    [DataField] public ProtoId<BossMusicPrototype> SoundId;
}
