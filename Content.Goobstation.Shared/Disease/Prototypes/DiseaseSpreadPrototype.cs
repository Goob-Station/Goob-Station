using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Disease
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
        private string Name { get; set; } = default!;

        [ViewVariables(VVAccess.ReadOnly)]
        public string LocalizedName => Loc.GetString("disease-spread-" + Name.ToLower());

        [DataField]
        public bool BlockedByInternals; // TODO: not implemented in the system
         }
}
