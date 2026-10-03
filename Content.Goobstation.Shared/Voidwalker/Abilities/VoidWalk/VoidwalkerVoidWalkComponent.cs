using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Voidwalker.Abilities.VoidWalk;

[RegisterComponent, AutoGenerateComponentState]
public sealed partial class VoidwalkerVoidWalkComponent : Component
{
    /// <summary>
    /// The ProtoID of the action so it can be added to the creature on compinit.
    /// </summary>
    [DataField]
    public EntProtoId VoidWalkAction = "ActionVoidwalkerVoidWalk";

    [DataField, AutoNetworkedField]
    public EntityUid? VoidWalkActionEntity;

    /// <summary>
    /// The distance travelled towards the targeted block.
    /// </summary>
    [DataField]
    public float Distance = 4.65f;

    /// <summary>
    /// The speed at which the dash travels.
    /// </summary>
    [DataField]
    public float Speed = 9.65f;
}
