namespace Content.Goobstation.Server.Cult.Components;

[RegisterComponent]
public sealed partial class BloodCultActionComponent : Component
{
    /// <summary>
    ///     Whether it should be deleted or not after depleting charges.
    /// </summary>
    public bool Limited = false;
}
