namespace Content.Goobstation.Shared.Movement.Pulling.Components;

public sealed partial class PullerComponent : Component
{
    /// <summary>
    /// Whether or not to apply speed modifiers to the puller
    /// </summary>
    [AutoNetworkedField, DataField]
    public bool ApplySpeedModifier = true;
}
