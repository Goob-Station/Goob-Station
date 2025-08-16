namespace Content.Shared._CorvaxGoob.TintedWindow;

[RegisterComponent, AutoGenerateComponentState]
public sealed partial class TintedWindowComponent : Component
{
    [DataField, AutoNetworkedField]
    public Angle Arc = 120;
}
