namespace Content.Server.Speech.Components;

[RegisterComponent]
public sealed partial class RodentiaAccentComponent : Component
{
    /// <summary>
    ///     Chance that the message will be appended with a mouse noise
    /// </summary>
    [DataField("squeakChance")]
    public float squeakChance = 0.3f; // Probably will tweak, depending!
}
