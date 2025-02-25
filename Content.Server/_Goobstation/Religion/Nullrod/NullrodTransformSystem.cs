using Content.Shared.Examine;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.Interaction;
using Content.Shared.Storage;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.Religion.Nullrod;

public sealed class NullrodTransformSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;


    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AltarSourceComponent, BeforeRangedInteractEvent>(OnBeforeInteract);
    }

    private void OnBeforeInteract(EntityUid uid, AltarSourceComponent component, BeforeRangedInteractEvent args)
    {


        if (args.Handled
            || !args.CanReach // prevent placing out of range
            || HasComp<StorageComponent>(args.Target) // if it's a storage component like a bag, we ignore usage so it can be stored
            || !TryComp<MetaDataComponent>(uid, out var metaData) //Grabs UID metadata
            )

                return;

                var proto = metaData.EntityPrototype;

                if (proto != null && proto.ID == component.InteractProto) { //Checks whether interacting object is as assigned in AltarSource YML

                    var nullrodUid = EntityManager.SpawnEntity(component.RodProto, args.ClickLocation.SnapToGrid(EntityManager));
                    var xform = Transform(nullrodUid); //Spawns entity assigned in RodProto
                    //QueueDel(uid);
                }



    }
}
