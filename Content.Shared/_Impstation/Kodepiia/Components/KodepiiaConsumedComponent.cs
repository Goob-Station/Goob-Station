using Robust.Shared.GameStates;

namespace Content.Shared._Impstation.Kodepiia.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class KodepiiaConsumedComponent : Component
{
    /// <summary>
    /// Consumed value, added to whenever a consumer consumes the consumed.
    /// </summary>
    [DataField]
    public float Count;
}
