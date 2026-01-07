using Content.Shared.Actions;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Doodons;

/// <summary>
/// Action event sent when Papa Doodon commands warriors
/// </summary>
public sealed partial class DoodonSetWarriorModeEvent : InstantActionEvent
{
    [DataField(required: true)]
    public DoodonOrderType Order;
}
