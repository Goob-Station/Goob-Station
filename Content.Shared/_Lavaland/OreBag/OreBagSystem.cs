using Content.Shared._DV.Salvage.Components;
using Content.Shared.Storage;
using Content.Shared.Interaction;
using Content.Shared.Materials;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Shared._Lavaland.OreBag;

public sealed class OreBagSystem : EntitySystem
{
    [Dependency] private readonly SharedMaterialStorageSystem _materialStorage = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<OreBagComponent, AfterInteractEvent>(OnAfterInteract);
    }

    private void OnAfterInteract(EntityUid uid, OreBagComponent component, AfterInteractEvent args)
    {
        if (!args.CanReach
          || args.Target == null
          || !HasComp<MiningPointsLatheComponent>(args.Target)
          || !_timing.IsFirstTimePredicted)
            return;

        // Get the storage component that should be on the ore bag
        if (!TryComp<StorageComponent>(uid, out var storage))
            return;

        var validEntities = new List<EntityUid>();

        // Find all valid entities (ones with MaterialComponent)
        foreach (var entity in storage.Container.ContainedEntities)
        {
            if (HasComp<MaterialComponent>(entity))
                validEntities.Add(entity);
        }

        bool transferred = false;
        // Transfer all valid entities to the lathe
        foreach (var entity in validEntities)
        {
            if (_materialStorage.TryInsertMaterialEntity(args.User, entity, args.Target.Value))
                transferred = true;
        }

        if (transferred)
            _audio.PlayPredicted(new SoundPathSpecifier("/Audio/Effects/Cargo/ping.ogg"), args.Target.Value, args.User);
    }
}
