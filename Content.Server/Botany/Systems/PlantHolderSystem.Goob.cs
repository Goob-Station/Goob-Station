using Content.Goobstation.Common.Botany;

namespace Content.Server.Botany.Systems;

public sealed partial class PlantHolderSystem
{
    private void GoobRaiseHarvested(EntityUid plantHolder, EntityUid user)
    {
        var ev = new PlantHarvestedEvent(plantHolder);
        RaiseLocalEvent(user, ref ev);
    }
}
