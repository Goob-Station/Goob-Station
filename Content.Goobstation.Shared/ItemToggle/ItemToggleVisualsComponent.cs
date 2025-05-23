using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.ItemToggle;

[RegisterComponent]
public sealed partial class ItemToggleVisualsComponent : Component
{
    [DataField]
    public string? HeldPrefixOn = "on";

    [DataField]
    public string? HeldPrefixOff = "off";
}

[Serializable, NetSerializable]
public enum ItemToggleVisuals
{
    State,
    Layer,
}
