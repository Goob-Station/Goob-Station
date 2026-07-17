using Content.Shared.Actions;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.CosmicCult;

[RegisterComponent, NetworkedComponent]
public sealed partial class CosmicCultActionComponent : Component;

public sealed partial class EventCosmicSiphon : EntityTargetActionEvent;
public sealed partial class EventCosmicBlank : EntityTargetActionEvent;
public sealed partial class EventCosmicPlaceMonument : InstantActionEvent; //given to the cult leader on roundstart
public sealed partial class EventCosmicMoveMonument : InstantActionEvent; //given the the cult leader on hitting tier 1, taken away on hitting tier 3
public sealed partial class EventCosmicReturn : InstantActionEvent;
public sealed partial class EventCosmicLapse : EntityTargetActionEvent;
public sealed partial class EventCosmicGlare : InstantActionEvent;
public sealed partial class EventCosmicIngress : EntityTargetActionEvent;
public sealed partial class EventCosmicImposition : InstantActionEvent;
public sealed partial class EventCosmicNova : WorldTargetActionEvent;
public sealed partial class EventCosmicFragmentation : EntityTargetActionEvent;

// COLOSSUS ACTIONS
public sealed partial class EventCosmicColossusSunder : WorldTargetActionEvent;
public sealed partial class EventCosmicColossusIngress : EntityTargetActionEvent;
public sealed partial class EventCosmicColossusHibernate : InstantActionEvent;
public sealed partial class EventCosmicColossusEffigy : InstantActionEvent;
