using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wraith.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class BloodCrayonComponent : Component
{
    /// <summary>
    /// Wraith Points to consume on use
    /// </summary>
    [DataField(required: true)]
    public int WpConsume;
}
