using Content.Shared.Antag;
using Robust.Shared.GameStates;
using Content.Shared.StatusIcon;
using Robust.Shared.Prototypes;
using Robust.Shared.Audio;

namespace Content.Shared.Revolutionary.Components;

/// <summary>
/// Used for marking regular revs as well as storing icon prototypes so you can see fellow revs.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedRevolutionarySystem))]
public sealed partial class RevolutionaryComponent : Component
{
    /// <summary>
    /// The status icon prototype displayed for revolutionaries
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public ProtoId<StatusIconPrototype> StatusIcon { get; set; } = "RevolutionaryFaction";

    /// <summary>
    /// Sound that plays when you are chosen as Rev.
    /// </summary>
    [DataField]
    public SoundSpecifier RevStartSound = new SoundPathSpecifier("/Audio/Goobstation/Ambience/Antag/rev_start.ogg"); // Goobstation - add custom rev conversion noise :)

    public override bool SessionSpecific => true;
}
