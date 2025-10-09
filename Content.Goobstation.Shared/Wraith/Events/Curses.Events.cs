using Content.Shared.Actions;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Events;

/// <summary>
///  Marks an action as a CurseAction
/// </summary>
[RegisterComponent]
public sealed partial class CurseActionComponent : Component;

/// <summary>
/// Used in actions for applying curses
/// </summary>
public sealed partial class ApplyCurseActionEvent : EntityTargetActionEvent
{
    [DataField]
    public ComponentRegistry? Curse = new();

    [DataField]
    public SoundSpecifier? CurseSound;
};

/// <summary>
/// Raised before a curse gets applied on an entity
/// </summary>
/// <param name="Target"></param> The target trying to apply the curse to
[ByRefEvent]
public record struct AttemptCurseEvent(bool Cancelled = false);
