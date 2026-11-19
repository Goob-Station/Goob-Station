using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Bongle;

/// <summary>
/// To run multiple gamerules at once.
/// Idk if this exists somewhere already but  coudldn't find it.
/// Probably not too useful outside of bingle bongle wars, its specifically because they need two differnet spawn points.
/// </summary>
[RegisterComponent]
public sealed partial class ChainRulesRuleComponent : Component
{
    [DataField(required: true)]
    public List<EntProtoId> Rules = new();
}
