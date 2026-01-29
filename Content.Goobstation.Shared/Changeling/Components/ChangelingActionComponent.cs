using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Changeling.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ChangelingActionComponent : Component
{
    [DataField]
    public LocId OnFirePopup = "changeling-action-fail-onfire";

    [DataField]
    public LocId LesserFormPopup = "changeling-action-fail-lesserform";

    [DataField]
    public LocId InvalidChemicalsPopup = "changeling-chemicals-deficit";

    [DataField]
    public LocId InsufficientAbsorbsPopup = "changeling-action-fail-absorbed";

    [DataField]
    public LocId NotChangelingPopup = "changeling-action-fail-not-changeling";

    [DataField, AutoNetworkedField]
    public float ChemicalCost = 0;

    [DataField, AutoNetworkedField]
    public float AltChemicalCost = 0; // specifically for actions with an on/off state

    [DataField, AutoNetworkedField]
    public bool UseOnFire = false;

    [DataField, AutoNetworkedField]
    public bool UseInLastResort = false;

    [DataField, AutoNetworkedField]
    public bool UseInLesserForm = false;

    [DataField, AutoNetworkedField]
    public float RequireAbsorbed = 0;
}
