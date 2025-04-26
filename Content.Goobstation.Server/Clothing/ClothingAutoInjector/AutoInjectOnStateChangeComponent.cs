namespace Content.Goobstation.Server.Clothing.ClothingAutoInjector;

[RegisterComponent]
public sealed partial class AutoInjectOnStateChangeComponent : Component
{
    /// <summary>
    /// The autoinjector this component is linked to.
    /// </summary>
    public EntityUid? ClothingAutoInjector;
}
