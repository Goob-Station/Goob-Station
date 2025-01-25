using Content.Shared._Shitmed.Targeting;

namespace Content.Server.Atmos.Components;

/// <summary>
///   Component that is used on clothing to prevent ignition when exposed to a specific gas.
/// </summary>
[RegisterComponent]
public sealed partial class IgniteFromGasImmunityComponent : Component
{
    // <summary>
    //   Which body parts are given ignition immunity.
    // </summary>
    [DataField(required: true)]
    public HashSet<TargetBodyPart> Parts;
}
