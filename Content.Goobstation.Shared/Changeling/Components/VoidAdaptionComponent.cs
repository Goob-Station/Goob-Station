using Content.Goobstation.Shared.InternalResources.Data;
using Content.Shared.Alert;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Changeling.Components;

/// <summary>
/// Marks a changeling that has evolved Void Adaption.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class VoidAdaptionComponent : Component
{
    [DataField]
    public ProtoId<AlertPrototype> Alert = "VoidAdaption";

    [DataField]
    public ProtoId<InternalResourcesPrototype> ResourceType = "ChangelingChemicals";

    [DataField]
    public bool FirePopupSent;

    [DataField]
    public LocId FirePopup = "changeling-voidadapt-onfire";

    [DataField]
    public bool AdaptingLowPressure;

    [DataField]
    public LocId EnterLowPressurePopup = "changeling-voidadapt-lowpressure-start";

    [DataField]
    public LocId LeaveLowPressurePopup = "changeling-voidadapt-lowpressure-end";

    [DataField]
    public bool AdaptingLowTemp;

    [DataField]
    public LocId EnterLowTempPopup = "changeling-voidadapt-lowtemperature-start";

    [DataField]
    public LocId LeaveLowTempPopup = "changeling-voidadapt-lowtemperature-end";

    [DataField]
    public float ChemModifierValue = 0.25f;
}
