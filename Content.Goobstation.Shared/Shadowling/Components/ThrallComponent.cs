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
    [DataField]
    public float EnthrallDurationEffect = 1.5f;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public ProtoId<FactionIconPrototype> StatusIcon { get; set; } = "ThrallFaction";

    public readonly List<ProtoId<EntityPrototype>> BaseThrallActions = new()
    {
        "ActionThrallDarksight",
        "ActionGuise"
    };

    public string? ActionThrallDarksight = "ActionThrallDarksight";
    public string? ActionGuise = "ActionGuise";

    public EntityUid? ActionThrallDarksightEntity;
    public EntityUid? ActionGuiseEntity;

    [DataField]
    public bool NightVisionMode;

    [DataField]
    public SoundSpecifier? ThrallConverted = new SoundPathSpecifier("/Audio/_EinsteinEngines/Shadowling/thrall.ogg");

    /// <summary>
    /// The shadowling that converted the Thrall
    /// </summary>
    [DataField]
    public EntityUid? Converter;
}
