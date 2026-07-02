using Robust.Shared.Audio;

namespace Content.Shared._Pirate.Plumbing.Components;

/// <summary>
///     Marker component for plumbing machines that can be plunged.
///     When a plunger is used on this entity, all solution containers are emptied onto the floor.
/// </summary>
[RegisterComponent]
public sealed partial class PlungeableComponent : Component
{
    /// <summary>
    ///     Sound played when the machine is successfully drained.
    /// </summary>
    [DataField]
    public SoundSpecifier DrainSound = new SoundPathSpecifier("/Audio/Effects/Fluids/slosh.ogg");
}
