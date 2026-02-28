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
    /// <summary>
    /// The action entity for roaring
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? RoarActionEntity;

    /// <summary>
    /// The roar action prototype
    /// </summary>
    [DataField]
    public EntProtoId RoarAction = "ActionQueenroar";

    /// <summary>
    /// Sound played when roaring
    /// </summary>
    [DataField]
    public SoundSpecifier? SoundRoar = new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_queen_screech.ogg")
    {
        Params = AudioParams.Default.WithVolume(-2f)
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
    public float RoarStunTime = 6f;

    /// <summary>
    /// How long the roar takes to charge up
    /// </summary>
    [DataField]
    public float RoarDelay = 3f;
}
