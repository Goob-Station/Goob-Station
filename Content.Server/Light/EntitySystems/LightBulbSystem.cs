// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2022 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2022 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Destructible;
using Content.Shared.Light.Components;
using Content.Shared.Throwing;
using Robust.Shared.Audio.Systems;

namespace Content.Server.Light.EntitySystems
{
    public sealed class LightBulbSystem : EntitySystem
    {
        [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<LightBulbComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<LightBulbComponent, LandEvent>(HandleLand);
            SubscribeLocalEvent<LightBulbComponent, BreakageEventArgs>(OnBreak);
        }

        private void OnInit(EntityUid uid, LightBulbComponent bulb, ComponentInit args)
        {
            // update default state of bulbs
            SetColor(uid, bulb.Color, bulb);
            SetState(uid, bulb.State, bulb);
        }

        private void HandleLand(EntityUid uid, LightBulbComponent bulb, ref LandEvent args)
        {
            PlayBreakSound(uid, bulb);
            SetState(uid, LightBulbState.Broken, bulb);
        }

        private void OnBreak(EntityUid uid, LightBulbComponent component, BreakageEventArgs args)
        {
            SetState(uid, LightBulbState.Broken, component);
        }

        /// <summary>
        ///     Set a new color for a light bulb and raise event about change
        /// </summary>
        public void SetColor(EntityUid uid, Color color, LightBulbComponent? bulb = null)
        {
            if (!Resolve(uid, ref bulb))
                return;

            bulb.Color = color;
            UpdateAppearance(uid, bulb);
        }

        /// <summary>
        ///     Set a new state for a light bulb (broken, burned) and raise event about change
        /// </summary>
        public void SetState(EntityUid uid, LightBulbState state, LightBulbComponent? bulb = null)
        {
            if (!Resolve(uid, ref bulb))
                return;

            bulb.State = state;
            UpdateAppearance(uid, bulb);
        }

        public void PlayBreakSound(EntityUid uid, LightBulbComponent? bulb = null)
        {
            if (!Resolve(uid, ref bulb))
                return;

            _audio.PlayPvs(bulb.BreakSound, uid);
        }

        private void UpdateAppearance(EntityUid uid, LightBulbComponent? bulb = null,
            AppearanceComponent? appearance = null)
        {
            if (!Resolve(uid, ref bulb, ref appearance, logMissing: false))
                return;

            // try to update appearance and color
            _appearance.SetData(uid, LightBulbVisuals.State, bulb.State, appearance);
            _appearance.SetData(uid, LightBulbVisuals.Color, bulb.Color, appearance);
        }
    }
}