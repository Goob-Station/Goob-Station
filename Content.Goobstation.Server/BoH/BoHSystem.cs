using Content.Server.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Interaction;

namespace Content.Goobstation.Server.BoH
{
    public sealed class BoHSystem : EntitySystem
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IAdminLogManager _adminLogger = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<BoHComponent, InteractUsingEvent>(OnInteract);
        }

        private void OnInteract(EntityUid uid, BoHComponent component, InteractUsingEvent args)
        {
            _entityManager.TryGetComponent<BoHComponent>(args.Used, out var second_boh);
            if (second_boh == null)
                return;

            var newUid = Spawn("Singularity", Transform(uid).Coordinates);

            _adminLogger.Add(
                LogType.Action, LogImpact.Low,
                $"{ToPrettyString(args.User):actor} created {ToPrettyString(newUid)} by putting {ToPrettyString(args.Used)} into {ToPrettyString(uid)}");

            QueueDel(args.Used);
        }
    }
}
