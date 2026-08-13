using Robust.Shared.GameStates;

namespace Content.Shared._Shitmed.Medical.Surgery.Traumas.Components;

[RegisterComponent]
public sealed partial class TraumaBloodlossComponent : Component
{
    /// <summary>
    /// Constant bleed floor this trauma maintains on the body while embedded.
    /// </summary>
    [DataField]
    public float Amount = 0.6f;
}
