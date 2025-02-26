using Content.Shared.Examine;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.Interaction;
using Content.Shared.Storage;
using Robust.Shared.Prototypes;
using Content.Shared.Tag;

namespace Content.Server._Goobstation.Religion.Nullrod;

public sealed class NullrodTransformSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly TagSystem _tagSystem = default!;


    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AltarSourceComponent, InteractUsingEvent>(OnInteractUsing);
    }

    private void OnInteractUsing(EntityUid uid, AltarSourceComponent component, InteractUsingEvent args)
    {


        if (args.Handled
            || HasComp<StorageComponent>(args.Target) // if it's a storage component like a bag, we ignore usage so it can be stored.
            || !_tagSystem.HasTag(args.Used, "Nullrod") //Checks used entity for the tag we need.
            )
                return;


                var nullrodUid = Spawn(component.RodProto, args.ClickLocation.SnapToGrid(EntityManager));
                var xform = Transform(nullrodUid); //Spawns entity assigned in RodProto.

                QueueDel(args.Used); //deletes old entity.
                args.Handled = true;
    }
}
