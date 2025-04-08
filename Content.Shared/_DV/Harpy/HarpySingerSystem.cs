// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;

namespace Content.Shared._DV.Harpy
{
    public class HarpySingerSystem : EntitySystem
    {
        [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<HarpySingerComponent, ComponentStartup>(OnStartup);
            SubscribeLocalEvent<HarpySingerComponent, ComponentShutdown>(OnShutdown);
        }

        private void OnStartup(EntityUid uid, HarpySingerComponent component, ComponentStartup args)
        {
            _actionsSystem.AddAction(uid, ref component.MidiAction, component.MidiActionId);
        }

        private void OnShutdown(EntityUid uid, HarpySingerComponent component, ComponentShutdown args)
        {
            _actionsSystem.RemoveAction(uid, component.MidiAction);
        }
    }
}
