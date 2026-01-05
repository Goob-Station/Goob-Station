using Content.Shared.StatusIcon;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Shadowling.Components;

/// <summary>
/// This is used for marking Thralls and storing their icons
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ThrallComponent : Component
{
    [ViewVariables]
    public EntityUid? Converter;

    [DataField]
    public ProtoId<FactionIconPrototype> StatusIcon { get; set; } = "ThrallFaction";

    [DataField]
    public SoundSpecifier? ThrallConverted = new SoundPathSpecifier("/Audio/_EinsteinEngines/Shadowling/thrall.ogg");
}
