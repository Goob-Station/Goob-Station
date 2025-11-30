using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Disease
{
    /// <summary>
    ///     A type of disease. Determines effects this disease can mutate and medicines that work against it.
    /// </summary>
    [Prototype]
    public sealed partial class DiseaseTypePrototype : IPrototype
    {
        [IdDataField]
        public string ID { get; private set; } = default!;

        [DataField(required: true)]
        private LocId Name { get; set; }

        [ViewVariables(VVAccess.ReadOnly)]
        public string LocalizedName => Loc.GetString(Name);
    }
}
