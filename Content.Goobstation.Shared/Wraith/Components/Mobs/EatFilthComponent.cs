using Content.Shared.Chemistry.Reagent;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Components.Mobs;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class EatFilthComponent : Component
{
    /// <summary>
    /// Used to keep track of how much trash the rat has eaten.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int FilthConsumed;

    /// <summary>
    /// How long it takes to eat
    /// </summary>
    [DataField]
    public TimeSpan EatDuration = TimeSpan.FromSeconds(8);

    /// <summary>
    ///  The list of reagents that the puddle has to contain in order to be consumed
    /// </summary>
    [DataField]
    public List<ProtoId<ReagentPrototype>>? AllowedReagents = new();

    /// <summary>
    ///  What the rat is allowed to eat
    /// </summary>
    [DataField]
    public EntityWhitelist? AllowedEntities = new();
}

/// <summary>
/// Raised once you eat filth
/// </summary>
/// <param name="CurrentFilthConsumed"></param> The current filth that has been consumed by the entity
[ByRefEvent]
public record struct AteFilthEvent(int CurrentFilthConsumed);
