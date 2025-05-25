using Robust.Shared.Prototypes;

namespace Content.Goobstation.Prototypes
{
    [Prototype("cookingRecipe")]
    public sealed class CookingRecipePrototype : IPrototype
    {
        [IdDataField] public string ID { get; } = string.Empty;

        [DataField("name")]
        public string Name = string.Empty;

        [DataField("input")]
        public List<string> Input = new();

        [DataField("output")]
        public string Output = string.Empty;

        [DataField("requiredMachine")]
        public string RequiredMachine = string.Empty;

        [DataField("requiredTime")]
        public float RequiredTime = 1f;
    }
}