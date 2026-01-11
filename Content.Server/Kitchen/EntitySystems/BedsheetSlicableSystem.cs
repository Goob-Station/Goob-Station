using Content.Server.Kitchen.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Robust.Shared.Containers;
using Robust.Shared.Random;
using Content.Shared.Random;

namespace Content.Server.Kitchen.EntitySystems;

public sealed class BedsheetSlicableSystem : EntitySystem
{
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly RandomHelperSystem _randomHelper = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BedSheetSlicableComponent, InteractUsingEvent>(OnInteractUsing);
    }

    private void OnInteractUsing(EntityUid uid, BedSheetSlicableComponent component, InteractUsingEvent args)
    {
        if (!HasComp<SharpComponent>(args.Used))
            return;

        // if in some container, try pick up, else just drop to world
        var inContainer = _containerSystem.IsEntityInContainer(uid);
        var pos = Transform(uid).Coordinates;

        var count = _random.Next(component.SpawnCountMin, component.SpawnCountMax + 1);

        for (var i = 0; i < count; i++)
        {
            var cloth = Spawn(component.SpawnedPrototype, pos);

            if (inContainer)
                _handsSystem.PickupOrDrop(args.User, cloth);
            else
            {
                var xform = Transform(cloth);
                _containerSystem.AttachParentToContainerOrGrid((cloth, xform));
                xform.LocalRotation = 0;
                _randomHelper.RandomOffset(cloth, 0.25f);
            }
        }

        QueueDel(uid);
        args.Handled = true;
    }
}