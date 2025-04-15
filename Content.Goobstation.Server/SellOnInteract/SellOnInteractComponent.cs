using Content.Shared.Stacks;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Goobstation.Server.SellOnInteract;

[RegisterComponent]
public sealed partial class SellOnInteractComponent : Component
{
    /// <summary>
    /// Can this item sell structures?
    /// </summary>
    [DataField]
    public bool CanSellStructures;

    [DataField(customTypeSerializer:typeof(PrototypeIdSerializer<StackPrototype>))]
    public string CashType = "Credit";

    /// <summary>
    /// Self-explanatory
    /// </summary>
    [DataField]
    public TimeSpan DoAfterDuration = TimeSpan.FromSeconds(5);
}
