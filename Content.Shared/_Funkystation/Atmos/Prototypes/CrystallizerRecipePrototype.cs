using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._Funkystation.Atmos.Prototypes
{
    [Prototype("crystallizerRecipe")]
    public sealed class CrystallizerRecipePrototype : IPrototype
    {
        [IdDataField]
        public string ID { get; private set; } = default!;

        [DataField("name")]
        public string Name { get; private set; } = default!;

        [DataField("minimumTemperature")]
        public float MinimumTemperature { get; private set; }

        [DataField("maximumTemperature")]
        public float MaximumTemperature { get; private set; }

        [DataField("minimumRequirements")]
        public float[] MinimumRequirements { get; private set; } = default!;

        [DataField("energyRelease")]
        public float EnergyRelease { get; private set; }

        [DataField("products")]
        public Dictionary<string, int> Products { get; private set; } = new();

        [DataField("dangerous")]
        public bool Dangerous { get; private set; }
    }
}