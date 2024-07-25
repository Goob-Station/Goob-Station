using Content.Shared.Actions;
using Content.Shared.Changeling.Components;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Changeling;

[RegisterComponent, NetworkedComponent]
public sealed partial class ChangelingActionComponent : Component
{
    [DataField] public float ChemicalCost = 0;

    [DataField] public bool UseInLesserForm = false;

    [DataField] public float RequireAbsorbed = 0;

    [DataField] public bool Audible = false;
}

/// <summary>
///     Used for custom changeling action behavior. Every other event dedidcated to changelings should be used here.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ChangelingActionBehaviorCustomComponent : Component
{
    [DataField] public object Event;
}

/// <summary>
///     Used for changeling sting handling.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ChangelingActionBehaviorStingComponent : Component
{
    [DataField] public bool TargetSelf = false;
    [DataField] public Dictionary<EntProtoId, FixedPoint2> Reagents = new();
}

/// <summary>
///     Used for changeling equipment handling.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ChangelingActionBehaviorEquipComponent : Component
{
    [DataField] public List<ChangelingEquipmentData> Equipment;
}

[RegisterComponent, NetworkedComponent]
public sealed partial class ChangelingActionBehaviorShriekComponent : Component
{
    [DataField] public SoundSpecifier ShriekSound = new SoundPathSpecifier("/Audio/Goobstation/Changeling/Effects/changeling_shriek.ogg");
    /// <summary>
    ///     Power of the shriek. Determines how far will it reach and how much will it influent it's targets.
    /// </summary>
    [DataField] public float Power = 2.5f;
}

/// <summary>
///     Base instant event that should be used in actions in combination with ChangelingActionBehaviors.
/// </summary>
public sealed partial class ChangelingInstantActionEvent : InstantActionEvent { }
/// <summary>
///     Base target event that should be used in actions in combination with ChangelingActionBehaviors.
/// </summary>
public sealed partial class ChangelingTargetActionEvent : EntityTargetActionEvent { }

#region Custom behavior

public sealed partial class OpenEvolutionMenuEvent : InstantActionEvent { }
public sealed partial class AbsorbDNAEvent : EntityTargetActionEvent { }
public sealed partial class StingExtractDNAEvent : EntityTargetActionEvent { }
public sealed partial class ChangelingTransformCycleEvent : InstantActionEvent { }
public sealed partial class ChangelingTransformEvent : InstantActionEvent { }
public sealed partial class EnterStasisEvent : InstantActionEvent { }
public sealed partial class ExitStasisEvent : InstantActionEvent { }
public sealed partial class ShriekDissonantEvent : InstantActionEvent { }
public sealed partial class ShriekResonantEvent : InstantActionEvent { }
public sealed partial class ToggleStrainedMusclesEvent : InstantActionEvent { }
public sealed partial class StingBlindEvent : EntityTargetActionEvent { }
public sealed partial class StingFakeArmbladeEvent : EntityTargetActionEvent { }
public sealed partial class StingTransformEvent : EntityTargetActionEvent { }
public sealed partial class ActionAugmentedEyesightEvent : InstantActionEvent { }
public sealed partial class ActionBiodegradeEvent : InstantActionEvent { }
public sealed partial class ActionChameleonSkinEvent : InstantActionEvent { }
public sealed partial class ActionEphedrineOverdoseEvent : InstantActionEvent { }
public sealed partial class ActionLesserFormEvent : InstantActionEvent { }
public sealed partial class ActionHivemindAccessEvent : InstantActionEvent { }

#endregion
