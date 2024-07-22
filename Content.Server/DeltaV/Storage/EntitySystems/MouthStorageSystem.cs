using System.Linq;
using Content.Server.Nutrition;
using Content.Server.Speech;
using Content.Server.Speech.EntitySystems;
using Content.Shared.DeltaV.Storage.Components;
using Content.Shared.DeltaV.Storage.EntitySystems;
using Content.Shared.Examine;
using Content.Shared.IdentityManagement;
using Content.Shared.Storage;

namespace Content.Server.DeltaV.Storage.EntitySystems;

public sealed class MouthStorageSystem : SharedMouthStorageSystem
{
    [Dependency] private readonly ReplacementAccentSystem _replacement = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MouthStorageComponent, AccentGetEvent>(OnAccent);
        SubscribeLocalEvent<MouthStorageComponent, IngestionAttemptEvent>(OnIngestAttempt);
        SubscribeLocalEvent<MouthStorageComponent, ExaminedEvent>(OnExamined);
    }

    // Force you to mumble if you have items in your mouth
    private void OnAccent(EntityUid uid, MouthStorageComponent component, AccentGetEvent args)
    {
        if (IsMouthBlocked(component))
            return;

        args.Message = _replacement.ApplyReplacements(args.Message, "mumble");
    }

    // Attempting to eat or drink anything with items in your mouth won't work
    private void OnIngestAttempt(EntityUid uid, MouthStorageComponent component, IngestionAttemptEvent args)
    {
        if (!IsMouthBlocked(component))
            return;

        if (!TryComp<StorageComponent>(component.MouthId, out var storage))
            return;

        var firstItem = storage.Container.ContainedEntities[0];
        args.Blocker = firstItem;
        args.Cancel();
    }

    // Other people can see if this person has items in their mouth.
    private void OnExamined(EntityUid uid, MouthStorageComponent component, ExaminedEvent args)
    {
        if (IsMouthBlocked(component))
        {
            var entityManager = IoCManager.Resolve<IEntityManager>();
            var subject = Identity.Entity(uid, entityManager);
            args.PushMarkup(Loc.GetString("mouth-storage-examine-condition-occupied", ("entity", subject)));
        }
    }
}