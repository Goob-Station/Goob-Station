using Robust.Shared.Prototypes;

namespace Content.Shared.Disease
{
    /// <summary>
    ///     A type of disease spread.
    /// </summary>
    [Prototype]
    public sealed partial class DiseaseSpreadPrototype : IPrototype
    {
        [IdDataField]
        public string ID { get; private set; } = default!;

        [DataField(required: true)]
        private LocId Name { get; set; }

        [ViewVariables(VVAccess.ReadOnly)]
        public string LocalizedName => Loc.GetString(Name);
    }
}
