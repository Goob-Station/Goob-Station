using Content.Shared.Stacks;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Goobstation.Server.SellOnInteract;

[RegisterComponent]
public sealed partial class SellOnInteractComponent : Component
{
    /// <summary>
    /// Can this item sell ANYTHING (ignore all blacklists)
    /// </summary>
    [DataField]
    public bool CanSellAnything;

    [DataField(customTypeSerializer:typeof(PrototypeIdSerializer<StackPrototype>))]
    public string CashType = "Credit";

    /// <summary>
    /// Self-explanatory
    /// </summary>
    [DataField]
    public TimeSpan DoAfterDuration = TimeSpan.FromSeconds(5);
}
