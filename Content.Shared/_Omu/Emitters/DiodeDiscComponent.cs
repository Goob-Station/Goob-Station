using Content.Shared.DoAfter;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Content.Shared._DV.Construction;

namespace Content.Shared._Omu.DiodeDisc;

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

    [DataField]
    public EntityUid? SoundStream;

    [DataField]
    public EntProtoId NewBolt;
}

[RegisterComponent]
public sealed partial class AngeringProjectileComponent : Component
{
    [DataField]
    public float? IntegDamage;

    [DataField]
    public float? EnergyDamage;
}

[Serializable, NetSerializable]
public sealed partial class DiodeDiscDoAfterEvent : SimpleDoAfterEvent;
