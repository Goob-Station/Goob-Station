using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.DoAfter;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CombatDoAfterComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public ushort? DoAfterId;

    [ViewVariables, AutoNetworkedField]
    public EntityUid? DoAfterUser;

    [DataField(required: true)]
    public BaseCombatDoAfterSuccessEvent Trigger;

    [DataField]
    public float Delay = 1.5f;

    [DataField]
    public float DelayVariation = 0.3f;

    [DataField]
    public float ActivationTolerance = 0.3f;

    [DataField]
    public bool Hidden;

    [DataField]
    public bool BreakOnMove;

    [DataField]
    public bool BreakOnWeightlessMove;

    [DataField]
    public bool BreakOnDamage;

    [DataField]
    public bool MultiplyDelay;

    [DataField]
    public Color? ColorOverride = Color.Red;

    [DataField]
    public bool MeleeHitTrigger;
}
