using Content.Goobstation.Shared.Changeling.Components;
using Content.Goobstation.Shared.Changeling.Systems;
using Content.Server.Polymorph.Systems;
using Content.Shared.Polymorph;

namespace Content.Goobstation.Server.Changeling;

public sealed partial class ChangelingChemicalSystem : SharedChangelingChemicalSystem
{
    [Dependency] private readonly PolymorphSystem _polymorph = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangelingChemicalComponent, PolymorphedEvent>(OnPolymorphed);
    }

    private void OnPolymorphed(Entity<ChangelingChemicalComponent> ent, ref PolymorphedEvent args)
    {
        _polymorph.CopyPolymorphComponent<ChangelingChemicalComponent>(ent, args.NewEntity);

        // have to manually copy over the InternalResourcesData stuff
        var oldData = Comp<ChangelingChemicalComponent>(args.OldEntity).ResourceData;

        var newComp = Comp<ChangelingChemicalComponent>(args.NewEntity);
        var newData = newComp.ResourceData;

        newData.CurrentAmount = oldData.CurrentAmount;
        newData.MaxAmount = oldData.MaxAmount;
        newData.RegenerationRate = oldData.RegenerationRate;
        newData.InternalResourcesType = oldData.InternalResourcesType;
    }
}
