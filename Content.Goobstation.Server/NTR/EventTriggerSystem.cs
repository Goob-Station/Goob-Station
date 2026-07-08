// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.GameTicking;

namespace Content.Goobstation.Server.NTR
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
                _gt.StartGameRule(component.EventId, out _);
        }
    }
}
