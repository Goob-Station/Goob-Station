// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 LuciferEOS <stepanteliatnik2022@gmail.com>
// SPDX-FileCopyrightText: 2025 LuciferMkshelter <stepanteliatnik2022@gmail.com>
//
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
