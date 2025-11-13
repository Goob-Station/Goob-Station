using Robust.Shared.GameStates;

namespace Content.Shared._Shitcode.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class HereticBladeUserBonusDamageComponent : Component
{
    [DataField]
    public float BonusMultiplier = 0.5f;

    [DataField]
    public string? Path = "Flesh";
}
