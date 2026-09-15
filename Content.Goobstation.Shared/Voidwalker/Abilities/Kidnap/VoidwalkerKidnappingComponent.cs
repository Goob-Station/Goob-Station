namespace Content.Goobstation.Shared.Voidwalker.Abilities.Kidnap;

[RegisterComponent]
public sealed partial class VoidwalkerKidnappingComponent : Component
{
    /// <summary>
    /// How long does it take to send ya?
    /// </summary>
    [DataField]
    public TimeSpan KidnapDoAfterDuration = TimeSpan.FromSeconds(5);

    /// <summary>
    /// How long are we sendin' ya fer?
    /// </summary>
    [DataField]
    public TimeSpan KidnapDuration = TimeSpan.FromSeconds(30);
}
