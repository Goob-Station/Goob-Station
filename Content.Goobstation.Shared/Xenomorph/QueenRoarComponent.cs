using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Xenomorph;

/// <summary>
/// Allows the xenomorph queen to roar and stun nearby enemies
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class QueenRoarComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public NetEntity? RoarActionEntity;

    [DataField]
    public EntProtoId RoarAction = "ActionQueenRoar";

    [DataField]
    public SoundSpecifier? SoundRoar = new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_queen_screech.ogg")
    {
        Params = AudioParams.Default.WithVolume(-2f)
        .WithMaxDistance(15f),
    };

    [DataField]
    public SoundSpecifier? SoundRoarStart = new SoundPathSpecifier("/Audio/_Goobstation/Effects/queen_roar_start.ogg")
    {
        Params = AudioParams.Default.WithVolume(12f)
    .WithMaxDistance(15f),
    };

    /// <summary>
    /// Range of the roar effect in tiles
    /// </summary>
    [DataField]
    public float RoarRange = 6f;

    /// <summary>
    /// How long enemies are stunned for
    /// </summary>
    [DataField]
    public TimeSpan RoarStunTime = TimeSpan.FromSeconds(6);

    /// <summary>
    /// How long the roar takes to charge up
    /// </summary>
    [DataField]
    public TimeSpan RoarDelay = TimeSpan.FromSeconds(3);
}
