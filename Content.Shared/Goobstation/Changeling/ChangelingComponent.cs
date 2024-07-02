using Robust.Shared.GameStates;

namespace Content.Shared.Changeling;

[RegisterComponent, NetworkedComponent]
public sealed partial class ChangelingComponent : Component
{
    [DataField("chemicals")]
    public float Chemicals = 100;

    [DataField("maxChemicals")]
    public float MaxChemicals = 100;
}
