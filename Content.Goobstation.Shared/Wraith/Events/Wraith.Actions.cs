using Content.Shared.Actions;
using Content.Shared.DoAfter;

namespace Content.Goobstation.Shared.Wraith.Events;

// Here belong all action events for the base wraith

public sealed partial class HauntEvent : InstantActionEvent
{
}
public sealed partial class WhisperEvent : InstantActionEvent
{
}
public sealed partial class BloodWritingEvent : InstantActionEvent
{
}
public sealed partial class AbsorbCorpseEvent : SimpleDoAfterEvent
{
    public EntityUid Target;
}
public sealed partial class SpookEvent : InstantActionEvent
{
}
public sealed partial class DecayEvent : InstantActionEvent
{
}
public sealed partial class CommandEvent : InstantActionEvent
{
}
public sealed partial class AnimateObjectEvent : InstantActionEvent
{
}
public sealed partial class PossessObjectEvent : InstantActionEvent
{
}
public sealed partial class WraithEvolveEvent : InstantActionEvent
{
}
