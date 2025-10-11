using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitcode.Heretic.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class StarGazerComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid Summoner;

    [DataField]
    public float MaxDistance = 20f;

    [DataField]
    public float GhostRoleTimer = 10f;

    [ViewVariables(VVAccess.ReadWrite)]
    public float GhostRoleAccumulator;

    [DataField]
    public float ResetDistanceTimer = 5f;

    [ViewVariables(VVAccess.ReadWrite)]
    public float ResetDistanceAccumulator;

    [DataField]
    public EntProtoId TeleportEffect = "EffectCosmicCloud";

    [DataField]
    public SoundSpecifier TeleportSound = new SoundPathSpecifier("/Audio/_Goobstation/Heretic/cosmic_energy.ogg");
}
