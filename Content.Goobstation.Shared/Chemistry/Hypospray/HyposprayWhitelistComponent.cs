using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Chemistry.Hypospray;

/// <summary>
/// Prevents a hypospray from drawing or injecting non whitelisted reagents.
/// </summary>
[RegisterComponent, AutoGenerateComponentState]
public sealed partial class HyposprayWhitelistComponent : Component
{

    [DataField, AutoNetworkedField]
    public HashSet<ProtoId<ReagentPrototype>> Whitelist = [];

    [DataField]
    public LocId DrawFailureMessage;

    [DataField]
    public LocId InjectFailureMessage;
}
