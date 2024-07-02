using Content.Shared.Actions;

namespace Content.Shared.Changeling;

#region Events

public sealed partial class OpenEvolutionMenuEvent : InstantActionEvent { }

public sealed partial class AbsorbDNAEvent : WorldTargetActionEvent { }

public sealed partial class StingExtractDNAEvent : WorldTargetActionEvent { }

public sealed partial class ChangelingTransformEvent : InstantActionEvent { }

public sealed partial class EnterStasisEvent : InstantActionEvent { }

public sealed partial class ExitStasisEvent : InstantActionEvent { }

#endregion
