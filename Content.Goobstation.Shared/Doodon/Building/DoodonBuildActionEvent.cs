using Content.Shared.Actions;

namespace Content.Goobstation.Shared.Doodon.Building;

public sealed partial class DoodonBuildCycleActionEvent : InstantActionEvent { }

public sealed partial class DoodonBuildActionEvent : EntityTargetActionEvent { }

public sealed partial class DoodonBuildWorldActionEvent : WorldTargetActionEvent { }

public sealed partial class DoodonOpenBuildMenuActionEvent : InstantActionEvent { }

public sealed partial class DoodonBuildToggleOrOpenMenuEvent : InstantActionEvent { }
