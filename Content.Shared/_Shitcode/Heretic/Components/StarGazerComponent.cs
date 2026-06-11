using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Content.Shared.StatusIcon;

namespace Content.Shared._Shitcode.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class StarGazerComponent : Component
{
    [DataField]
    public ProtoId<FactionIconPrototype> MasterIcon = "GhoulHereticMaster";

    [DataField]
    public float MaxDistance = 20f;

    [ViewVariables, NonSerialized]
    public ICommonSession? ResettingMindSession;

    [DataField]
    public float GhostRoleTimer = 20f;

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
