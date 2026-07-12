using Robust.Shared.GameStates;

namespace Content.Shared._Impstation.Consume.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class ConsumedComponent : Component
{
    /// <summary>
    /// Consumed value, added to whenever a consumer consumes the consumed.
    /// </summary>
    [DataField]
    public float ConsumedValue;
}
