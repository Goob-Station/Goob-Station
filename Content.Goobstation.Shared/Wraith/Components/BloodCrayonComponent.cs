namespace Content.Goobstation.Shared.Wraith.Components;
//Ported from Impstation
[RegisterComponent]
public sealed partial class BloodCrayonComponent : Component
{
    /// <summary>
    /// Wraith Points to consume on use
    /// </summary>
    [DataField(required: true)]
    public int WPConsume;
}
