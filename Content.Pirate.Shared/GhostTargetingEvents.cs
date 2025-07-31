using Content.Shared.Actions;

namespace Content.Pirate.Shared
{

    public sealed partial class ClearTargetGhostActionEvent : InstantActionEvent {}

    public sealed partial class SetTargetGhostActionEvent : EntityTargetActionEvent {}

    public sealed partial class ToggleGhostFormActionEvent : InstantActionEvent {}

    public sealed partial class GhostBlinkActionEvent : InstantActionEvent { }

}

