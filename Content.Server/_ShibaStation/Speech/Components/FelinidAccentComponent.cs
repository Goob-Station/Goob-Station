namespace Content.Server.Speech.Components;

[RegisterComponent]
public sealed partial class FelinidAccentComponent : Component
{
    /// <summary>
    ///     Chance that the message will be appended with a cat noise
    /// </summary>
    [DataField("meowChance")]
    public float meowChance = 0.3f; // Probably will tweak, depending!
}
