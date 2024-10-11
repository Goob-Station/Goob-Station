using Content.Shared._Goobstation.Changeling.EntitySystems;
using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Changeling;

[RegisterComponent, NetworkedComponent]
public sealed partial class ChangelingActionComponent : Component
{
    [DataField]
    public float ChemicalCost = 0;

    [DataField]
    public float BiomassCost = 0;

    [DataField]
    public ChangelingFormType RequiredFormType = ChangelingFormType.NoForm;

    [DataField]
    public float RequireAbsorbed = 0;
}

#region Events - Basic

public sealed partial class OpenEvolutionMenuEvent : InstantActionEvent { }
public sealed partial class AbsorbDNAEvent : ChangelingEntityTargetActionEvent { }
public sealed partial class StingExtractDNAEvent : EntityTargetActionEvent { }
public sealed partial class ChangelingTransformCycleEvent : InstantActionEvent { }
public sealed partial class ChangelingTransformEvent : InstantActionEvent { }
public sealed partial class ToggleChangelingStasisEvent : BaseAlertEvent { }
#endregion

#region Events - Combat

public sealed partial class ToggleArmbladeEvent : InstantActionEvent { }
public sealed partial class CreateBoneShardEvent : InstantActionEvent { }
public sealed partial class ToggleChitinousArmorEvent : InstantActionEvent { }
public sealed partial class ToggleOrganicShieldEvent : InstantActionEvent { }
public sealed partial class ShriekDissonantEvent : InstantActionEvent { }
public sealed partial class ShriekResonantEvent : InstantActionEvent { }
public sealed partial class ToggleStrainedMusclesEvent : InstantActionEvent { }

#endregion

#region Events - Sting
public sealed partial class ChangelingStingEvent : ChangelingEntityTargetActionEvent
{
    [DataField(required: true)]
    public ProtoId<StingPrototype> Sting { get; set; }
}

[Serializable, NetSerializable]
public sealed partial class ReagentStingEvent : EntityEventArgs
{
    [DataField(required: true)]
    public Dictionary<string, FixedPoint2> Reagents { get; set; } = default!;

    public float Latency { get; set; }
}

public sealed partial class StingBlindEvent : EntityTargetActionEvent { }
public sealed partial class StingCryoEvent : EntityTargetActionEvent { }
public sealed partial class StingLethargicEvent : EntityTargetActionEvent { }
public sealed partial class StingMuteEvent : EntityTargetActionEvent { }
public sealed partial class StingFakeArmbladeEvent : EntityTargetActionEvent { }
public sealed partial class StingTransformEvent : EntityTargetActionEvent { }
public sealed partial class StingLayEggsEvent : EntityTargetActionEvent { }

#endregion

#region Events - Utility

[Serializable, NetSerializable]
public sealed partial class BuyAugmentedEyesEvent : EntityEventArgs { }

[Serializable, NetSerializable]
public sealed partial class BuyHivemindAccessEvent : EntityEventArgs { }

public sealed partial class ActionAnatomicPanaceaEvent : InstantActionEvent { }
public sealed partial class ActionBiodegradeEvent : InstantActionEvent { }
public sealed partial class ActionChameleonSkinEvent : InstantActionEvent { }
public sealed partial class ActionEphedrineOverdoseEvent : InstantActionEvent { }
public sealed partial class ActionFleshmendEvent : InstantActionEvent { }
public sealed partial class ActionLastResortEvent : InstantActionEvent { }
public sealed partial class ActionLesserFormEvent : InstantActionEvent { }
public sealed partial class ActionSpacesuitEvent : InstantActionEvent { }
public sealed partial class ActionHivemindAccessEvent : InstantActionEvent { }
public sealed partial class ActionContortBodyEvent : InstantActionEvent { }

#endregion

public abstract partial class ChangelingEntityTargetActionEvent : EntityTargetActionEvent, IChangelingAction
{
    [DataField]
    public float ChemicalCost { get; set; }

    [DataField]
    public float BiomassCost { get; set; }

    [DataField]
    public float RequiredAbsorbed { get; set; }

    [DataField]
    public ChangelingFormType RequiredForm { get; set; }
}

public abstract partial class ChangelingInstantActionEvent : InstantActionEvent, IChangelingAction
{
    [DataField]
    public float ChemicalCost { get; set; }

    [DataField]
    public float BiomassCost { get; set; }

    [DataField]
    public float RequiredAbsorbed { get; set; }

    [DataField]
    public ChangelingFormType RequiredForm { get; set; }
}

/// <summary>
///     Interface that uses to give biomass/chemicals price for events
/// </summary>
public interface IChangelingAction
{
    public float ChemicalCost { get; set; }

    public float BiomassCost { get; set; }

    public ChangelingFormType RequiredForm { get; set; }

    public float RequiredAbsorbed { get; set; }
}
