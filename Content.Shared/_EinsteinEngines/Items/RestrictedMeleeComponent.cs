// <summary>
//   Locks an item to only be used in melee by entities with a specific component.
// </summary>

using Content.Shared.Whitelist;
using Robust.Shared.Audio;


[RegisterComponent]
public sealed partial class RestrictedMeleeComponent : Component
{
    [DataField]
    public EntityWhitelist? Whitelist;

    [DataField]
    public TimeSpan KnockdownDuration = TimeSpan.FromSeconds(2);

    [DataField]
    public string FailText { get; set; } = "restricted-melee-component-attack-fail-too-large";

    [DataField]
    public bool DoKnockdown = true;

    [DataField]
    public bool ForceDrop = true;

    [DataField]
    public SoundSpecifier FallSound = new SoundPathSpecifier("/Audio/Effects/slip.ogg");
}
