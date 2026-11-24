using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Bongle;

/// <summary>
/// Is capable of running multiple gamerules at once.
/// Theoretically not the best way to be doing so, but if you wish for a gamerule
/// to spawn two different oposing factions, this should do the trick.
/// </summary>
[RegisterComponent]
public sealed partial class ChainRulesRuleComponent : Component
{
    [DataField(required: true)]
    public List<EntProtoId> Rules = new();
}
