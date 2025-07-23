using Robust.Shared.GameObjects;
using Content.Shared.Actions;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.TypeSerializers.Implementations;
using Robust.Shared.Network;
using Robust.Shared.Utility;
using System;

namespace Content.Pirate.Shared
{

    public sealed partial class ClearTargetGhostActionEvent : InstantActionEvent {}

    public sealed partial class SetTargetGhostActionEvent : EntityTargetActionEvent {}

    public sealed partial class ToggleGhostFormActionEvent : InstantActionEvent {}

    public sealed partial class GhostBlinkActionEvent : InstantActionEvent { }

}

