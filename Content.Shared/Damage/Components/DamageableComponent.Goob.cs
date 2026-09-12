namespace Content.Shared.Damage.Components;

public sealed partial class DamageableComponent
{
    [ViewVariables]
    public TimeSpan LastModifiedTime = TimeSpan.Zero;
}