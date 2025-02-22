using Robust.Shared.GameStates;

namespace Content.Shared.Traits.Assorted;

/// <summary>
/// Rolls a chance to gib the entity every second.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(RandomGibSystem))]
public sealed partial class RandomGibComponent : Component
{
    [DataField]
    public float Chance = 1 / 3600f; // ~63.2% chance of having gibbed an hour in
}
