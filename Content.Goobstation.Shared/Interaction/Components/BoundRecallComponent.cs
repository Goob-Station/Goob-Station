using Robust.Shared.Audio;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Interaction.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BoundRecallComponent : Component
{
    /// <summary>
    /// Gets or sets the unique identifier of the user entity that is bound to this object, if any.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? BoundUser;

    /// <summary>
    /// Gets or sets the entity prototype identifier used for the recall action.
    /// </summary>
    /// <remarks>This field specifies the prototype ID that represents the recall action for a bound item.
    /// Changing this value affects which action is triggered when a recall is performed.</remarks>
    [DataField]
    public EntProtoId RecallAction = "ActionRecallBoundItem";
}
