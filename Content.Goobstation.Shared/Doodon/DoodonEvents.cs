using Content.Shared.Actions;

namespace Content.Goobstation.Shared.Doodons;

/// <summary>
/// Action event for seeing town hall's influence radius
/// </summary>
public sealed partial class ToggleTownHallRadiusEvent : InstantActionEvent { }

/// <summary>
/// Action event for establishing the town hall
/// </summary>
public sealed partial class DoodonEstablishTownHallEvent : InstantActionEvent { }

/// <summary>
/// Action event for commanding doodon warriors
/// </summary>
public sealed partial class PapaDoodonCommandActionEvent : InstantActionEvent
{
}

/// <summary>
/// Action event sent when Papa Doodon commands warriors
/// </summary>
public sealed partial class DoodonSetWarriorModeEvent : InstantActionEvent
{
    [DataField(required: true)]
    public DoodonOrderType Order;
}

