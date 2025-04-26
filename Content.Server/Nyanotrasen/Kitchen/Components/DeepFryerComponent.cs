using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Construction.Prototypes;
using Content.Shared.EntityEffects;
using Content.Shared.FixedPoint;
using Content.Shared.Nutrition;
using Content.Shared.Nyanotrasen.Kitchen;
using Content.Shared.Nyanotrasen.Kitchen.Components;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Set;

namespace Content.Server.Nyanotrasen.Kitchen.Components
{
    [RegisterComponent]
    [Access(typeof(SharedDeepfryerSystem))]
    // This line appears to be depracted: [ComponentReference(typeof(SharedDeepFryerComponent))]
    public sealed partial class DeepFryerComponent : SharedDeepFryerComponent
    {
        // There are three levels to how the deep fryer treats entities.
        //
        // 1. An entity can be rejected by the blacklist and be untouched by
        //    anything other than heat damage.
        //
        // 2. An entity can be deep-fried but not turned into an edible. The
        //    change will be mostly cosmetic. Any entity that does not match
        //    the blacklist will fall into this category.
        //
        // 3. An entity can be deep-fried and turned into something edible. The
        //    change will permit the item to be permanently destroyed by eating
        //    it.

        /// <summary>
        /// When will the deep fryer layer on the next stage of crispiness?
        /// </summary>
        [DataField("nextFryTime", customTypeSerializer: typeof(TimeOffsetSerializer))]
        public TimeSpan NextFryTime { get; set; }

        /// <summary>
        /// How much waste needs to be added at the next update interval?
        /// </summary>
        public FixedPoint2 WasteToAdd { get; set; } = FixedPoint2.Zero;

        /// <summary>
        /// How often are items in the deep fryer fried?
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("fryInterval")]
        public TimeSpan FryInterval { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// What entities cannot be deep-fried no matter what?
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("blacklist")]
        public EntityWhitelist? Blacklist { get; set; }

        /// <summary>
        /// What entities can be deep-fried into being edible?
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("whitelist")]
        public EntityWhitelist? Whitelist { get; set; }

        /// <summary>
        /// What are over-cooked and burned entities turned into?
        /// </summary>
        /// <remarks>
        /// To prevent unwanted destruction of items, only food can be turned
        /// into this.
        /// </remarks>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("charredPrototype", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string? CharredPrototype { get; set; }

        /// <summary>
        /// What reagents are considered valid cooking oils?
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("fryingOils", customTypeSerializer: typeof(PrototypeIdHashSetSerializer<ReagentPrototype>))]
        public HashSet<string> FryingOils { get; set; } = new();

        /// <summary>
        /// What reagents are added to tasty deep-fried food?
        /// JJ Comment: I removed Solution from this. Unsure if I need to replace it with something.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("goodReagents")]
        public List<ReagentQuantity> GoodReagents { get; set; } = new();

        /// <summary>
        /// What reagents are added to terrible deep-fried food?
        /// JJ Comment: I removed Solution from this. Unsure if I need to replace it with something.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("badReagents")]
        public List<ReagentQuantity> BadReagents { get; set; } = new();

        /// <summary>
        /// What reagents replace every 1 unit of oil spent on frying?
        /// JJ Comment: I removed Solution from this. Unsure if I need to replace it with something.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("wasteReagents")]
        public List<ReagentQuantity> WasteReagents { get; set; } = new();

        /// <summary>
        /// What flavors go well with deep frying?
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("goodFlavors", customTypeSerializer: typeof(PrototypeIdHashSetSerializer<FlavorPrototype>))]
        public HashSet<string> GoodFlavors { get; set; } = new();

        /// <summary>
        /// What flavors don't go well with deep frying?
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("badFlavors", customTypeSerializer: typeof(PrototypeIdHashSetSerializer<FlavorPrototype>))]
        public HashSet<string> BadFlavors { get; set; } = new();

        /// <summary>
        /// How much is the price coefficiency of a food changed for each good flavor?
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("goodFlavorPriceBonus")]
        public float GoodFlavorPriceBonus { get; set; } = 0.2f;

        /// <summary>
        /// How much is the price coefficiency of a food changed for each bad flavor?
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("badFlavorPriceMalus")]
        public float BadFlavorPriceMalus { get; set; } = -0.3f;

        /// <summary>
        /// What is the name of the solution container for the fryer's oil?
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("solution")]
        public string SolutionName { get; set; } = "vat_oil";

        public Solution Solution { get; set; } = default!;

        /// <summary>
        /// What is the name of the entity container for items inside the deep fryer?
        /// </summary>
        [DataField("storage")]
        public string StorageName { get; set; } = "vat_entities";

        public BaseContainer Storage { get; set; } = default!;

        /// <summary>
        /// How much solution should be imparted based on an item's size?
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("solutionSizeCoefficient")]
        public FixedPoint2 SolutionSizeCoefficient { get; set; } = 1f;

        /// <summary>
        /// What's the maximum amount of solution that should ever be imparted?
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("solutionSplitMax")]
        public FixedPoint2 SolutionSplitMax { get; set; } = 10f;

        /// <summary>
        /// What percent of the fryer's solution has to be oil in order for it to fry?
        /// </summary>
        /// <remarks>
        /// The chef will have to clean it out occasionally, and if too much
        /// non-oil reagents are added, the vat will have to be drained.
        /// </remarks>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("fryingOilThreshold")]
        public FixedPoint2 FryingOilThreshold { get; set; } = 0.5f;

        /// <summary>
        /// What is the bare minimum number of oil units to prevent the fryer
        /// from unsafe operation?
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("safeOilVolume")]
        public FixedPoint2 SafeOilVolume { get; set; } = 10f;

        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("unsafeOilVolumeEffects")]
        public List<EntityEffect> UnsafeOilVolumeEffects = new();

        /// <summary>
        /// What is the temperature of the vat when the deep fryer is powered?
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("poweredTemperature")]
        public float PoweredTemperature = 550.0f;

        /// <summary>
        /// How many entities can this deep fryer hold?
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public int StorageMaxEntities = 4;

        /// <summary>
        /// How many entities can be held, at a minimum?
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("baseStorageMaxEntities")]
        public int BaseStorageMaxEntities = 4;

        /// <summary>
        /// What upgradeable machine part dictates the quality of the storage size?
        /// </summary>
        [DataField("machinePartStorageMax", customTypeSerializer: typeof(PrototypeIdSerializer<MachinePartPrototype>))]
        public string MachinePartStorageMax = "MatterBin";

        /// <summary>
        /// How much extra storage is added per part rating?
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("storagePerPartRating")]
        public int StoragePerPartRating = 4;

        /// <summary>
        /// What sound is played when an item is inserted into hot oil?
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("soundInsertItem")]
        public SoundSpecifier SoundInsertItem = new SoundPathSpecifier("/Audio/Nyanotrasen/Machines/deepfryer_basket_add_item.ogg");

        /// <summary>
        /// What sound is played when an item is removed?
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("soundRemoveItem")]
        public SoundSpecifier SoundRemoveItem = new SoundPathSpecifier("/Audio/Nyanotrasen/Machines/deepfryer_basket_remove_item.ogg");
    }
}
