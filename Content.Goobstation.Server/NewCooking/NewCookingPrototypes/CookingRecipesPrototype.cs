using Robust.Shared.Prototypes;

namespace Content.Goobstation.Prototypes
{
    [Prototype("cookingRecipe")]
    [DataDefinition]
    public sealed partial class CookingRecipePrototype : IPrototype
    {
        [IdDataField]
        public string ID { get; private set; } = default!;

        [DataField("name")]
        public string Name = string.Empty;

        [DataField("input")]
        public List<IngredientEntry> Input = new();

        [DataField("output")]
        public EntProtoId Output;

        [DataField("requiredMachine")]
        public string RequiredMachine = string.Empty;

        [DataField("requiredTime")]
        public float RequiredTime = 0f;

        [DataDefinition]
        public sealed partial class IngredientEntry
        {
            [DataField("id", required: true)]
            public EntProtoId ID = default!;

            [DataField("amount")]
            public int Amount = 1;
        }
    }
}