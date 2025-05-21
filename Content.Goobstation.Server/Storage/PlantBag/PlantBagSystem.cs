using Content.Server.Materials.Components;
using Content.Shared.Interaction;
using Content.Shared.Storage;

namespace Content.Goobstation.Server.Storage.PlantBag;

public sealed class PlantBagSystem : EntitySystem
{
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlantBagComponent, AfterInteractEvent>(OnAfterInteract);
    }

    private void OnAfterInteract(Entity<PlantBagComponent> ent, ref AfterInteractEvent args)
    {
        if (!args.CanReach
            || args.Target == null
            || !HasComp<ProduceMaterialExtractorComponent>(args.Target)
            || !TryComp<StorageComponent>(ent, out var storage))
            return;

        foreach (var produce in storage.Container.ContainedEntities)
        {
            _interaction.InteractDoAfter(args.User, produce, args.Target, Transform(args.Target.Value).Coordinates, true);
        }
    }
}
