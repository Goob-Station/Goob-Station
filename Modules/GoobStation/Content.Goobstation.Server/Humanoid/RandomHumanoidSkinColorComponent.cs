namespace Content.Goobstation.Server.Humanoid;

/// <summary>
///     Sets the attached entity's skin color to a random color in the supplied color palette
/// </summary>
[RegisterComponent]
public sealed partial class RandomHumanoidSkinColorComponent : Component
{
    /// <summary>
    ///     The color palette to use.
    /// </summary>
    [DataField(required: true)]
    public string Palette;
}
