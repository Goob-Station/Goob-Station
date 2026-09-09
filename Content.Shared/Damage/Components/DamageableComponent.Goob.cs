namespace Content.Shared.Damage;

public sealed partial class DamageableComponent
{
    [ViewVariables]
    public TimeSpan LastModifiedTime = TimeSpan.Zero;
}