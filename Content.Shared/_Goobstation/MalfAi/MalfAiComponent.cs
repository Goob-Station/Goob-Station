using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;
using Content.Shared.StatusIcon;

namespace Content.Shared.MalfAi;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class MalfAiComponent : Component
{

    public List<ProtoId<EntityPrototype>> BaseMalfAiActions = new()
    {
        "ModuleMenu",
        "CyborgHijack"
    };
    [DataField, AutoNetworkedField] public float ControlPower = 5f;
    [DataField, AutoNetworkedField] public float MaxControlPower = 250f;
    [ViewVariables(VVAccess.ReadOnly)] public bool AlreadyHijacking = false;
}
