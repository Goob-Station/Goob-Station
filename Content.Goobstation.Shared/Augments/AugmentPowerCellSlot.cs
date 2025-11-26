using Content.Shared.Alert;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Augments;

/// <summary>
/// Component for entities that serve as AugmentPowerCellSlot organs
/// <summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class AugmentPowerCellSlotComponent : Component
{
    /// <summary>
    /// The alert shown with the power level when a battery is installed.
    /// </summary>
    [DataField]
    public ProtoId<AlertPrototype> BatteryAlert = "AugmentBattery";

    /// <summary>
    /// The alert shown when no battery is installed.
    /// </summary>
    [DataField]
    public ProtoId<AlertPrototype> NoBatteryAlert = "BorgBatteryNone";
}

/// <summary>
/// Marker component to indicate that an entity currently has an AugmentPowerCellSlot organ
/// <summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class HasAugmentPowerCellSlotComponent : Component;

/// <summary>
/// Raised on all installed augments if a power cell slot augment loses power.
/// Also raised if an augment is installed with no power available.
/// </summary>
[ByRefEvent]
public record struct AugmentLostPowerEvent(EntityUid Body);

/// <summary>
/// Raised on all installed augments if a power cell slot augment regains power.
/// Also raised if an augment is installed with power available.
/// </summary>
[ByRefEvent]
public record struct AugmentGainedPowerEvent(EntityUid Body);

/// <summary>
/// Alert event that tells the player their battery charge + power usage when clicked.
/// </summary>
public sealed partial class AugmentBatteryAlertEvent : BaseAlertEvent;
