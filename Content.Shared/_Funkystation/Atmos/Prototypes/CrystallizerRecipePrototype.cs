using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._Funkystation.Atmos.Prototypes
{
    [Prototype("crystallizerRecipe")]
    public sealed class CrystallizerRecipePrototype : IPrototype
    {
        [IdDataField]
        public string ID { get; private set; } = default!;

        [DataField]
        public string Name { get; private set; } = default!;

        [DataField]
        public float MinimumTemperature { get; private set; }

        [DataField]
        public float MaximumTemperature { get; private set; }

        [DataField]
        public float[] MinimumRequirements { get; private set; } = default!;

        [DataField]
        public float EnergyRelease { get; private set; }

        [DataField]
        public Dictionary<string, int> Products { get; private set; } = new();

        [DataField]
        public bool Dangerous { get; private set; }
    }
}
