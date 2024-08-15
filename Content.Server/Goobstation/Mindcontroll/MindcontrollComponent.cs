using Content.Shared.Antag;
using Robust.Shared.GameStates;
using Content.Shared.StatusIcon;
using Robust.Shared.Prototypes;
using Robust.Shared.Audio;

namespace Content.Server.Mindcontroll.Components;

[RegisterComponent]
public sealed partial class MindcontrollComponent : Component
{
    [DataField] public EntityUid? Master = null;
    [DataField] public EntityUid? Implant = null;
    [DataField] public SoundSpecifier MindcontrollStartSound = new SoundPathSpecifier("/Audio/Ambience/Antag/headrev_start.ogg"); // TODO Find a new sound.
}
