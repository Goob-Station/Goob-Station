// SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 theashtronaut <112137107+theashtronaut@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Kevin Zheng <kevinz5000@gmail.com>
// SPDX-FileCopyrightText: 2024 osjarw <62134478+osjarw@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Atmos.EntitySystems;
using Content.Server.Atmos.Piping.Components;
using Content.Server.NodeContainer;
using Content.Server.NodeContainer.EntitySystems;
using Content.Server.NodeContainer.Nodes;
using Content.Server.Popups;
using Content.Shared.Atmos;
using Content.Shared.Construction.Components;
using Content.Shared.Destructible;
using Content.Shared.Popups;
using JetBrains.Annotations;

namespace Content.Server.Atmos.Piping.EntitySystems
{
    [UsedImplicitly]
    public sealed class AtmosUnsafeUnanchorSystem : EntitySystem
    {
        [Dependency] private readonly AtmosphereSystem _atmosphere = default!;
        [Dependency] private readonly NodeGroupSystem _group = default!;
        [Dependency] private readonly PopupSystem _popup = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<AtmosUnsafeUnanchorComponent, UserUnanchoredEvent>(OnUserUnanchored);
            SubscribeLocalEvent<AtmosUnsafeUnanchorComponent, UnanchorAttemptEvent>(OnUnanchorAttempt);
            SubscribeLocalEvent<AtmosUnsafeUnanchorComponent, BreakageEventArgs>(OnBreak);
        }

        private void OnUnanchorAttempt(EntityUid uid, AtmosUnsafeUnanchorComponent component, UnanchorAttemptEvent args)
        {
            if (!component.Enabled || !EntityManager.TryGetComponent(uid, out NodeContainerComponent? nodes))
                return;

            if (_atmosphere.GetContainingMixture(uid, true) is not {} environment)
                return;

            foreach (var node in nodes.Nodes.Values)
            {
                if (node is not PipeNode pipe)
                    continue;

                if (pipe.Air.Pressure - environment.Pressure > 2 * Atmospherics.OneAtmosphere)
                {
                    args.Delay += 2f;
                    _popup.PopupEntity(Loc.GetString("comp-atmos-unsafe-unanchor-warning"), pipe.Owner,
                        args.User, PopupType.MediumCaution);
                    return; // Show the warning only once.
                }
            }
        }

        // When unanchoring a pipe, leak the gas that was inside the pipe element.
        // At this point the pipe has been scheduled to be removed from the group, but that won't happen until the next Update() call in NodeGroupSystem,
        // so we have to force an update.
        // This way the gas inside other connected pipes stays unchanged, while the removed pipe is completely emptied.
        private void OnUserUnanchored(EntityUid uid, AtmosUnsafeUnanchorComponent component, UserUnanchoredEvent args)
        {
            if (component.Enabled)
            {
                _group.ForceUpdate();
                LeakGas(uid);
            }
        }

        private void OnBreak(EntityUid uid, AtmosUnsafeUnanchorComponent component, BreakageEventArgs args)
        {
            LeakGas(uid, false);
            // Can't use DoActsBehavior["Destruction"] in the same trigger because that would prevent us
            // from leaking. So we make up for this by queueing deletion here.
            QueueDel(uid);
        }

        /// <summary>
        /// Leak gas from the uid's NodeContainer into the tile atmosphere.
        /// Setting removeFromPipe to false will duplicate the gas inside the pipe intead of moving it.
        /// This is needed to properly handle the gas in the pipe getting deleted with the pipe.
        /// </summary>
        public void LeakGas(EntityUid uid, bool removeFromPipe = true)
        {
            if (!EntityManager.TryGetComponent(uid, out NodeContainerComponent? nodes))
                return;

            if (_atmosphere.GetContainingMixture(uid, true, true) is not { } environment)
                environment = GasMixture.SpaceGas;

            var buffer = new GasMixture();

            foreach (var node in nodes.Nodes.Values)
            {
                if (node is not PipeNode pipe)
                    continue;

                if (removeFromPipe)
                    _atmosphere.Merge(buffer, pipe.Air.RemoveVolume(pipe.Volume));
                else
                {
                    var copy = new GasMixture(pipe.Air); //clone, then remove to keep the original untouched
                    _atmosphere.Merge(buffer, copy.RemoveVolume(pipe.Volume));
                }
            }

            _atmosphere.Merge(environment, buffer);
        }
    }
}