using Content.Server.Kitchen.EntitySystems;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Kitchen.Components;

[RegisterComponent]
[Access(typeof(BedsheetSlicableSystem))]
public sealed partial class BedSheetSlicableComponent : Component
{
    [DataField("spawnedPrototype", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string SpawnedPrototype = "MaterialCloth1";
    [DataField("spawnCountMin")] public int SpawnCountMin = 2;
    [DataField("spawnCountMax")] public int SpawnCountMax = 5;
}