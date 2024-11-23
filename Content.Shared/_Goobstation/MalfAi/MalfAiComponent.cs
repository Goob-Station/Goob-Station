using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Audio;

namespace Content.Shared.MalfAi;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class MalfAiComponent : Component
{

    public List<ProtoId<EntityPrototype>> BaseMalfAiActions = new()
    {
        "ModuleMenu",
        "CyborgHijack",
        "ProgramOverride"
    };
    [DataField, AutoNetworkedField] public float ControlPower = 10f;
    [DataField, AutoNetworkedField] public float MaxControlPower = 500f;
    [ViewVariables(VVAccess.ReadOnly)] public bool Hijacking = false;
    public SoundSpecifier AlarmSound = new SoundPathSpecifier("/Audio/Effects/Grenades/SelfDestruct/SDS_Charge.ogg");
}
