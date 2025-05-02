using Content.Shared.Actions;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Devil.Actions;
public sealed partial class CreateContractEvent : InstantActionEvent { }
public sealed partial class CreateRevivalContractEvent : InstantActionEvent { }
public sealed partial class ShadowJauntEvent : InstantActionEvent { }
public sealed partial class DevilPossessionEvent : EntityTargetActionEvent { }
