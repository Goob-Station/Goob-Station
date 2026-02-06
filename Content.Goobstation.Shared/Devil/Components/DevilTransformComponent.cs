using Content.Shared.Actions.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Devil.Components;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class DevilTransformComponent : Component
{
    [DataField]
    public EntProtoId Prototype = default!;

    /// Whether this is the lesser form.
    [DataField]
    public bool LesserForm;

    [DataField]
    public EntProtoId JauntAction = "ActionShadowJaunt";

    [DataField, AutoNetworkedField]
    public EntityUid? JauntActionEntity;

    [DataField]
    public EntProtoId HellfireAction = "ActionHellFire";

    [DataField, AutoNetworkedField]
    public EntityUid? HellfireActionEntity;

    [DataField]
    public EntProtoId ArchFireAction = "ActionHellFireArch";

    [DataField, AutoNetworkedField]
    public EntityUid? ArchFireActionEntity;

}
