using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._White.Xenomorphs.Acid;

/// <summary>
/// Added by the acid gland. Lets the owner switch spit ammo between neurotoxin and chemical heat.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class XenomorphSpitModeComponent : Component
{
    /// <summary>
    /// False = current neurotoxin spit. True = chemical heat spit (21 Heat, armor applies).
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool ChemicalMode;

    [DataField]
    public EntProtoId NeurotoxinProto = "BulletNeurotoxin";

    [DataField]
    public EntProtoId ChemicalProto = "BulletXenomorphAcidSpit";

    [DataField]
    public EntProtoId ToggleActionId = "ActionXenomorphSpitMode";

    [ViewVariables]
    public EntityUid? ToggleAction;
}

public sealed partial class XenomorphSpitModeToggleEvent : InstantActionEvent;
