using Content.Shared.Chasm;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Random;

namespace Content.Shared._Lavaland.Chasm;

public sealed class PreventChasmFallingSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PreventChasmFallingComponent, BeforeChasmFallingEvent>(OnBeforeFall);
        SubscribeLocalEvent<InventoryComponent, BeforeChasmFallingEvent>(Relay);
    }

    private void OnBeforeFall(EntityUid uid, PreventChasmFallingComponent comp, ref BeforeChasmFallingEvent args)
    {
        args.Cancelled = true;
        var coordsValid = false;
        var coords = Transform(args.Entity).Coordinates;

        const int attempts = 20;
        var curAttempts = 0;
        while (!coordsValid)
        {
            curAttempts++;
            if (curAttempts > attempts)
                return; // Just to be safe from stack overflow
            
            var newCoords = new EntityCoordinates(Transform(args.Entity).ParentUid, coords.X + _random.NextFloat(-5f, 5f), coords.Y + _random.NextFloat(-5f, 5f));
            if (!_interaction.InRangeUnobstructed(args.Entity, newCoords, -1f) ||
                _lookup.GetEntitiesInRange<ChasmComponent>(newCoords, 1f).Count > 0)
                continue;

            _transform.SetCoordinates(args.Entity, newCoords);
            _transform.AttachToGridOrMap(args.Entity, Transform(args.Entity));
            _audio.PlayPvs("/Audio/Items/Mining/fultext_launch.ogg", args.Entity);
            if (args.Entity != uid)
                QueueDel(uid);

            coordsValid = true;
        }
    }

    private void Relay(EntityUid uid, InventoryComponent comp, ref BeforeChasmFallingEvent args)
    {
        if (!HasComp<ContainerManagerComponent>(uid))
            return;

        RelayEvent(uid, ref args);
    }

    private void RelayEvent(EntityUid uid, ref BeforeChasmFallingEvent ev)
    {
        if (!TryComp<ContainerManagerComponent>(uid, out var containerManager))
            return;

        foreach (var container in containerManager.Containers.Values)
        {
            if (ev.Cancelled)
                break;

            foreach (var entity in container.ContainedEntities)
            {
                RaiseLocalEvent(entity, ref ev);
                if (ev.Cancelled)
                    break;
                RelayEvent(entity, ref ev);
            }
        }
    }
}
