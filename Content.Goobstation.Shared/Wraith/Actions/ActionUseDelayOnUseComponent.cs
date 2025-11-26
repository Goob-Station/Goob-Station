using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wraith.Actions;

[RegisterComponent, NetworkedComponent]
public sealed partial class ActionUseDelayOnUseComponent : Component
{
    /// <summary>
    /// How much use delay to add per action use
    /// </summary>
    [DataField(required: true)]
    public TimeSpan UseDelayAccumulator;

    /// <summary>
    /// The original use delay
    /// </summary>
    [ViewVariables]
    public TimeSpan OriginalUseDelay;
}
