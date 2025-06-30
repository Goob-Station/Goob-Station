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
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public ProtoId<FactionIconPrototype> StatusIcon { get; set; } = "ThrallFaction";

    public readonly List<ProtoId<EntityPrototype>> BaseThrallActions = new()
    {
        "ActionThrallDarksight",
        "ActionGuise"
    };

    public EntProtoId ActionThrallDarksight = "ActionThrallDarksight";
    public EntProtoId ActionGuise = "ActionGuise";

    public EntityUid? ActionThrallDarksightEntity;
    public EntityUid? ActionGuiseEntity;

    [DataField]
    public SoundSpecifier? ThrallConverted = new SoundPathSpecifier("/Audio/_EinsteinEngines/Shadowling/thrall.ogg");

    /// <summary>
    /// The shadowling that converted the Thrall
    /// </summary>
    [DataField]
    public EntityUid? Converter;
}
