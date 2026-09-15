using Content.Shared.Actions;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._White.Xenomorphs.HiveAnnounce;

/// <summary>
/// Psychic hive announcement. Granted automatically to entities with <see cref="Ovipositor.XenomorphOvipositorComponent"/>.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class XenomorphHiveAnnounceComponent : Component
{
    [DataField]
    public EntProtoId ActionId = "ActionXenomorphHiveAnnounce";

    [DataField]
    public SoundSpecifier? Sound = new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_queen_screech.ogg");

    [DataField]
    public int MaxLength = 256;

    [ViewVariables]
    public EntityUid? ActionEntity;
}

public sealed partial class XenomorphHiveAnnounceActionEvent : InstantActionEvent;
