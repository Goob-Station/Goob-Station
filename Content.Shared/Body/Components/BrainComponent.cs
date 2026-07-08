using Content.Shared.Body.Systems;

namespace Content.Shared.Body.Components;

[RegisterComponent, Access(typeof(BrainSystem))]
public sealed partial class BrainComponent : Component
{
    /// <summary>
    ///     Shitmed Change: Is this brain currently controlling the entity?
    /// </summary>
    [DataField]
    public bool Active = true;
}
