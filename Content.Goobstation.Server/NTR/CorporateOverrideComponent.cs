using Robust.Shared.Containers;

namespace Content.Goobstation.Server.NTR
{
    [RegisterComponent]
    public sealed partial class CorporateOverrideComponent : Component
    {
        [DataField]
        public string UnlockedCategory = "NTREvil";

        public ContainerSlot OverrideSlot = default!;
}
}
