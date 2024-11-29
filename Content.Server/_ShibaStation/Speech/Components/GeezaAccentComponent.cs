namespace Content.Server.Speech.Components;

[RegisterComponent]
public sealed partial class GeezaAccentComponent : Component
{
    /// <summary>
    ///     Chance that the message will be appended with "innit"
    /// </summary>
    [DataField("innitChance")]
    public float innitChance = 0.3f; // Probably will tweak, depending!
}
