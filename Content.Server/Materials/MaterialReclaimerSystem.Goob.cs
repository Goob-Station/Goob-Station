using Content.Goobstation.Common.Materials;

namespace Content.Server.Materials;

public sealed partial class MaterialReclaimerSystem
{
    private void GoobRaiseRecyclerGibbed(EntityUid recycler, EntityUid victim)
    {
        var ev = new RecyclerGibbedEvent(recycler);
        RaiseLocalEvent(victim, ref ev);
    }
}
