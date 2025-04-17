using Content.Shared.Roles;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server._Sunrise.AssaultOps;

[RegisterComponent]
public sealed partial class AssaultOperativeSpawnerComponent : Component
{
    [DataField("rolePrototype", customTypeSerializer:typeof(PrototypeIdSerializer<AntagPrototype>), required:true)]
    public string OperativeRolePrototype = default!;

    [DataField("startingGearPrototype", customTypeSerializer:typeof(PrototypeIdSerializer<StartingGearPrototype>), required:true)]
    public string OperativeStartingGear = default!;
}
