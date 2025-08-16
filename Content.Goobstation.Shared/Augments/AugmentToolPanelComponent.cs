using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Augments;

/// <summary>
/// Marker component to indicate that an entity will allow access to its storage via a radial menu once implanted
/// The storage must be filled before installation or you can't do anything with it.
/// An accessed item replaces a hand while in use and cannot be removed when stunned etc.
/// <summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class AugmentToolPanelComponent : Component
{
    /// <summary>
    /// Charge needed to switch items.
    /// </summary>
    [DataField]
    public float SwitchCharge = 10f;
}

/// <summary>
/// Marker component to indicate that an entity is the active tool of an augment tool panel
/// <summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class AugmentToolPanelActiveItemComponent : Component;

[Serializable, NetSerializable]
public enum AugmentToolPanelUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class AugmentToolPanelSwitchMessage(NetEntity? tool) : BoundUserInterfaceMessage
{
    public NetEntity? DesiredTool = tool;
}
