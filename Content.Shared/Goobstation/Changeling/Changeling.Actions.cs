using Content.Shared.Actions;
using Content.Shared.Changeling.Components;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Changeling;

[RegisterComponent, NetworkedComponent]
public sealed partial class ChangelingActionComponent : Component
{
    [DataField] public float ChemicalCost = 0;

    [DataField] public bool UseWhileLesserForm = false;

    [DataField] public float RequireAbsorbed = 0;

    [DataField] public bool Audible = false;
}

/// <summary>
///     Used for custom changeling action behavior. Every other event dedidcated to changelings should be used here.
/// </summary>
[RegisterComponent, NetworkedComponent]
public abstract partial class ChangelingActionBehaviorCustom : Component
{
    [DataField] public BaseActionEvent Event;
}

/// <summary>
///     Used for changeling sting handling.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ChangelingActionBehaviorSting : Component
{
    [DataField] public bool TargetSelf = false;
    [DataField] public Dictionary<EntProtoId, FixedPoint2> Reagents = new();
}

/// <summary>
///     Used for changeling equipment handling.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ChangelingActionBehaviorEquip : Component
{
    [DataField] public List<ChangelingEquipmentData> Equipment = new();
}

/// <summary>
///     Base instant event that should be used in actions in combination with ChangelingActionBehaviors.
/// </summary>
public sealed partial class ChangelingInstantActionEvent : InstantActionEvent { }
/// <summary>
///     Base target event that should be used in actions in combination with ChangelingActionBehaviors.
/// </summary>
public sealed partial class ChangelingTargetActionEvent : EntityTargetActionEvent { }

#region Events - Basic

public sealed partial class OpenEvolutionMenuEvent : InstantActionEvent { }
public sealed partial class AbsorbDNAEvent : EntityTargetActionEvent { }
public sealed partial class StingExtractDNAEvent : EntityTargetActionEvent { }
public sealed partial class ChangelingTransformCycleEvent : InstantActionEvent { }
public sealed partial class ChangelingTransformEvent : InstantActionEvent { }
public sealed partial class EnterStasisEvent : InstantActionEvent { }
public sealed partial class ExitStasisEvent : InstantActionEvent { }

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

public sealed partial class StingBlindEvent : EntityTargetActionEvent { }
public sealed partial class StingCryoEvent : EntityTargetActionEvent { }
public sealed partial class StingLethargicEvent : EntityTargetActionEvent { }
public sealed partial class StingMuteEvent : EntityTargetActionEvent { }
public sealed partial class StingFakeArmbladeEvent : EntityTargetActionEvent { }
public sealed partial class StingTransformEvent : EntityTargetActionEvent { }

#endregion

#region Events - Utility

public sealed partial class ActionAnatomicPanaceaEvent : InstantActionEvent { }
public sealed partial class ActionAugmentedEyesightEvent : InstantActionEvent { }
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
