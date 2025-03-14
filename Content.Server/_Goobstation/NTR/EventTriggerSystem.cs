using Content.Server.GameTicking;

namespace Content.Server._Goobstation.NTR
{
    public sealed class EventTriggerSystem : EntitySystem
    {
        [Dependency] private readonly GameTicker _gt = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<EventTriggerComponent, MapInitEvent>(OnMapInit);
        }

        private void OnMapInit(EntityUid uid, EventTriggerComponent component, MapInitEvent args)
        {
            if (!string.IsNullOrEmpty(component.EventId))
            {
                _gt.StartGameRule(component.EventId, out _);
            }
            // deleting the entityt after it has done its job of making a gamerule
            QueueDel(uid);
        }
    }
}
