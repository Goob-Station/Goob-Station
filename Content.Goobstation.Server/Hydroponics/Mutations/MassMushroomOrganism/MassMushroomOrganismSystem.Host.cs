using Content.Goobstation.Shared.Hydroponics.Mutations.MassMushroomOrganism;

namespace Content.Goobstation.Server.Hydroponics.Mutations.MassMushroomOrganism;

public partial class MassMushroomOrganismSystem
{
    private void InitializeMassMushroomOrganismHost()
    {
        SubscribeLocalEvent<MassMushroomOrganismHostComponent, FungalGrowthEvent>(OnFungalGrowth);
    }
    private void OnFungalGrowth(EntityUid uid, MassMushroomOrganismHostComponent component, FungalGrowthEvent args)
    {
        Log.Info("Hello");
    }
}
