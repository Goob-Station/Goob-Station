using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Dictionary;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Serialization; // Frontier

namespace Content.Shared.Kitchen
{
    /// <summary>
    ///    A recipe for space microwaves.
    /// </summary>
    [Prototype("microwaveMealRecipe")]
    public sealed partial class FoodRecipePrototype : IPrototype
    {
        [ViewVariables]
        [IdDataField]
        public string ID { get; private set; } = default!;

        [DataField("name")]
        private string _name = string.Empty;

        [DataField]
        public string Group = "Other";

        [DataField("reagents", customTypeSerializer:typeof(PrototypeIdDictionarySerializer<FixedPoint2, ReagentPrototype>))]
        private Dictionary<string, FixedPoint2> _ingsReagents = new();

        [DataField("solids", customTypeSerializer: typeof(PrototypeIdDictionarySerializer<FixedPoint2, EntityPrototype>))]
        private Dictionary<string, FixedPoint2> _ingsSolids = new ();

        [DataField("result", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string Result { get; private set; } = string.Empty;

        // Frontier
        [DataField]
        public int ResultCount { get; private set; } = 1;
        // End Frontier

        [DataField("time")]
        public uint CookTime { get; private set; } = 5;

        // Frontier: separate microwave recipe types.

        [DataField(required: true, customTypeSerializer: typeof(FlagSerializer<MicrowaveRecipeTypeFlags>))]
        public int RecipeType;

        [DataField]
        public bool HideInGuidebook;

        public string Name => Loc.GetString(_name);

        // TODO Turn this into a ReagentQuantity[]
        public IReadOnlyDictionary<string, FixedPoint2> IngredientsReagents => _ingsReagents;
        public IReadOnlyDictionary<string, FixedPoint2> IngredientsSolids => _ingsSolids;

        /// <summary>
        /// Is this recipe unavailable in normal circumstances?
        /// </summary>
        [DataField]
        public bool SecretRecipe = false;

        /// <summary>
        ///    Count the number of ingredients in a recipe for sorting the recipe list.
        ///    This makes sure that where ingredient lists overlap, the more complex
        ///    recipe is picked first.
        /// </summary>
        public FixedPoint2 IngredientCount()
        {
            FixedPoint2 n = 0;
            n += _ingsReagents.Count; // number of distinct reagents
            foreach (FixedPoint2 i in _ingsSolids.Values) // sum the number of solid ingredients
            {
                n += i;
            }
            return n;
        }
    }

    // Frontier: microwave recipe types, to limit certain recipes to certain machines
    [Flags, FlagsFor(typeof(MicrowaveRecipeTypeFlags))]
    [Serializable, NetSerializable]
    public enum MicrowaveRecipeType : int
    {
        Microwave = 1,
        Oven = 2,
        Assembler = 4,
        MedicalAssembler = 8,
    }

    public sealed class MicrowaveRecipeTypeFlags { }
}
