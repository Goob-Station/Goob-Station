using Robust.Shared.GameStates;

namespace Content.Shared._Shitcode.Heretic.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class HereticCombatMarkOnMeleeHitComponent : Component
{
    [DataField, AutoNetworkedField]
    public string NextPath = "Ash";

    [DataField]
    public List<string> Paths = new()
    {
        "Ash",
        "Void",
        "Flesh",
        "Blade",
        "Rust",
        "Cosmos",
    };
}
