using Content.Shared.StatusIcon;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Revolutionary;

[RegisterComponent, NetworkedComponent]
public sealed partial class RevolutionEnemyComponent : Component
{
    /// <summary>
    /// The status icon prototype displayed for revolutionaries
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public ProtoId<FactionIconPrototype> StatusIcon { get; set; } = "RevolutionaryFaction";

    /// <summary>
    /// Sound that plays when you are chosen as Rev. (Placeholder until I find something cool I guess)
    /// </summary>
    [DataField]
    public SoundSpecifier RevStartSound = new SoundPathSpecifier("/Audio/Ambience/Antag/headrev_start.ogg");

    public override bool SessionSpecific => true;
}
