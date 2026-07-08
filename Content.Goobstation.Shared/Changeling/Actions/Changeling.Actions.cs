// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Changeling.Actions;

#region Events - Basic

public sealed partial class OpenEvolutionMenuEvent : InstantActionEvent { }
public sealed partial class AbsorbDNAEvent : EntityTargetActionEvent { }
public sealed partial class AbsorbBiomatterEvent : EntityTargetActionEvent { }
public sealed partial class StingExtractDNAEvent : EntityTargetActionEvent { }
public sealed partial class ChangelingTransformCycleEvent : InstantActionEvent { }
public sealed partial class ChangelingTransformEvent : InstantActionEvent { }
public sealed partial class ChangelingRegenerateEvent : InstantActionEvent { }
public sealed partial class ChangelingStasisEvent : InstantActionEvent { }

#endregion

#region Events - Combat

public sealed partial class ToggleArmbladeEvent : InstantActionEvent { }
public sealed partial class ToggleArmHammerEvent : InstantActionEvent { }
public sealed partial class ToggleArmClawEvent : InstantActionEvent { }
public sealed partial class ToggleDartGunEvent : InstantActionEvent { }
public sealed partial class CreateBoneShardEvent : InstantActionEvent { }
public sealed partial class ToggleChitinousArmorEvent : InstantActionEvent { }
public sealed partial class ToggleOrganicShieldEvent : InstantActionEvent { }
public sealed partial class ShriekDissonantEvent : InstantActionEvent { }
public sealed partial class ShriekResonantEvent : InstantActionEvent { }
public sealed partial class ToggleStrainedMusclesEvent : InstantActionEvent { }

#endregion

#region Events - Sting

public sealed partial class StingReagentEvent : EntityTargetActionEvent { }
public sealed partial class StingFakeArmbladeEvent : EntityTargetActionEvent { }
public sealed partial class StingTransformEvent : EntityTargetActionEvent { }
public sealed partial class StingLayEggsEvent : EntityTargetActionEvent { }

#endregion

#region Events - Utility

public sealed partial class ActionAnatomicPanaceaEvent : InstantActionEvent
{
    [DataField]
    public ProtoId<AlertPrototype> Alert = "AnatomicPanacea";

    [DataField]
    public float Duration = 10f;
}
public sealed partial class ActionAugmentedEyesightEvent : InstantActionEvent { }
public sealed partial class ActionBiodegradeEvent : InstantActionEvent { }
public sealed partial class ActionChameleonSkinEvent : InstantActionEvent { }
public sealed partial class ActionDarknessAdaptionEvent : InstantActionEvent { }
public sealed partial class ActionAdrenalineReservesEvent : InstantActionEvent
{
    [DataField]
    public ProtoId<AlertPrototype> Alert = "AdrenalineReserves";

    [DataField]
    public float Duration = 10f;

    [DataField]
    public DamageSpecifier? PassiveDamage = new DamageSpecifier()
    {
        DamageDict =
        {
            { "Poison", 1.5 }
        }
    };
}
public sealed partial class ActionFleshmendEvent : InstantActionEvent
{
    [DataField]
    public ProtoId<AlertPrototype> Alert = "Fleshmend";

    [DataField]
    public SoundSpecifier PassiveSound = new SoundPathSpecifier("/Audio/_Goobstation/SpecialPassives/fleshmend_sfx.ogg");

    [DataField]
    public ResPath ResPath = new("_Goobstation/SpecialPassives/fleshmend_visuals.rsi");

    [DataField]
    public string EffectState = "mend_active";

    [DataField]
    public float Duration = 10f;
}
public sealed partial class ActionLastResortEvent : InstantActionEvent { }
public sealed partial class ActionLesserFormEvent : InstantActionEvent { }
public sealed partial class ActionHivemindAccessEvent : InstantActionEvent { }
public sealed partial class ActionContortBodyEvent : InstantActionEvent { }

#endregion
