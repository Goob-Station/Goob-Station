using Content.Shared.Singularity.EntitySystems;
using Content.Shared.DoAfter;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Content.Shared._DV.Construction;

namespace Content.Omu.Shared.DiodeDisc;

[RegisterComponent, NetworkedComponent, Access(typeof(UpgradeKitSystem))]
public sealed partial class DiodeDiscComponent : Component
{
    /// <summary>
    /// Components added to the machine after it's upgraded.
    /// Some of these must blacklist it from upgrades to prevent stacking.
    /// </summary>
    [DataField(required: true)]
    public ComponentRegistry ComponentsToAdd = new();

    /// <summary>
    /// How long the doafter is
    /// </summary>
    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Sound played when upgrading an entity.
    /// </summary>
    [DataField]
    public SoundSpecifier? UpgradeSound = new SoundPathSpecifier("/Audio/Items/rped.ogg");

    public EntityUid? SoundStream;

    [DataField]
    public EntProtoId NewBolt;
}

[Serializable, NetSerializable]
public sealed partial class DiodeDiscDoAfterEvent : SimpleDoAfterEvent;
