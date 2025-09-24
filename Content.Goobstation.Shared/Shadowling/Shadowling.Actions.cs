using Content.Shared.Actions;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Shadowling;

#region Events - First Phase

public sealed partial class HatchEvent : InstantActionEvent;

#endregion

#region Events - Second Phase

public sealed partial class EnthrallEvent : EntityTargetActionEvent;

public sealed partial class GlareEvent : EntityTargetActionEvent;

public sealed partial class VeilEvent : InstantActionEvent;

public sealed partial class RapidRehatchEvent : InstantActionEvent;

public sealed partial class ShadowWalkEvent : InstantActionEvent;

public sealed partial class IcyVeinsEvent : InstantActionEvent;

public sealed partial class DestroyEnginesEvent : InstantActionEvent;

public sealed partial class CollectiveMindEvent : InstantActionEvent;

#endregion

#region Events - Thrall Required

public sealed partial class BlindnessSmokeEvent : InstantActionEvent;

public sealed partial class NullChargeEvent : InstantActionEvent;

public sealed partial class SonicScreechEvent : InstantActionEvent;

public sealed partial class BlackRecuperationEvent : EntityTargetActionEvent;

public sealed partial class EmpoweredEnthrallEvent : EntityTargetActionEvent;

public sealed partial class NoxImperiiEvent : InstantActionEvent;
#endregion

#region Events - Ascension

public sealed partial class AscendanceEvent : InstantActionEvent;

public sealed partial class AnnihilateEvent : EntityTargetActionEvent;

public sealed partial class HypnosisEvent : EntityTargetActionEvent;

public sealed partial class TogglePlaneShiftEvent : InstantActionEvent;

public sealed partial class LightningStormEvent : InstantActionEvent;

public sealed partial class AscendantBroadcastEvent : InstantActionEvent;

#endregion

#region Events - Thrall Events

public sealed partial class GuiseEvent : InstantActionEvent;

#endregion
