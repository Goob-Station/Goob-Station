using Content.Shared.DeviceLinking;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server._Mono.SpaceArtillery.Components;

[RegisterComponent]
public sealed partial class SpaceArtilleryComponent : Component
{

    /// <summary>
    /// Passive power consumption drawn continuously from the powernet while the gun is operational.
    /// This represents baseline energy upkeep and is not tied to active firing.
    /// </summary>
    [DataField("powerUsePassive"), ViewVariables(VVAccess.ReadWrite)]
    public int PowerUsePassive = 600;

    /// <summary>
    /// Maximum rate at which the battery can recharge when connected to a powernet.
    /// Functions as a throttle for battery regeneration, consistent with BatterySelfRechargerComponent behavior.
    /// </summary>
    [DataField("powerChargeRate"), ViewVariables(VVAccess.ReadWrite)]
    public int PowerChargeRate = 3000;

    /// <summary>
    /// Additional power consumed per shot beyond the configured fire cost.
    /// This value is drained from the internal battery (or from the powernet if battery is insufficient).
    /// Used to simulate power-intensive firing beyond base projectile energy requirements.
    /// </summary>
    [DataField("powerUseActive"), ViewVariables(VVAccess.ReadWrite)]
    public int PowerUseActive = 6000;


    ///Sink Ports
    /// <summary>
    /// Signal port that makes space artillery fire.
    /// </summary>
    [DataField("spaceArtilleryFirePort", customTypeSerializer: typeof(PrototypeIdSerializer<SinkPortPrototype>))]
    public string SpaceArtilleryFirePort = "SpaceArtilleryFire";

}
