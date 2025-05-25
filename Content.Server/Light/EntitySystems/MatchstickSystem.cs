// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1+git@googlemail.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 Wrexbe <wrexbe@protonmail.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Atmos.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Item;
using Content.Shared.Smoking;
using Content.Shared.Smoking.Components; // Shitmed Change
using Content.Shared.Smoking.Systems; // Shitmed Change
using Content.Shared.Temperature;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;

namespace Content.Server.Light.EntitySystems
{
    public sealed class MatchstickSystem : SharedMatchstickSystem // Shitmed Change
    {
        [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
        [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;
        [Dependency] private readonly SharedItemSystem _item = default!;
        [Dependency] private readonly SharedPointLightSystem _lights = default!;
        [Dependency] private readonly TransformSystem _transformSystem = default!;

        private readonly HashSet<Entity<MatchstickComponent>> _litMatches = new();

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<MatchstickComponent, InteractUsingEvent>(OnInteractUsing);
            SubscribeLocalEvent<MatchstickComponent, IsHotEvent>(OnIsHotEvent);
            SubscribeLocalEvent<MatchstickComponent, ComponentShutdown>(OnShutdown);
        }

        private void OnShutdown(Entity<MatchstickComponent> ent, ref ComponentShutdown args)
        {
            _litMatches.Remove(ent);
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            foreach (var match in _litMatches)
            {
                if (match.Comp.CurrentState != SmokableState.Lit || Paused(match) || match.Comp.Deleted)
                    continue;

                var xform = Transform(match);

                if (xform.GridUid is not {} gridUid)
                    return;

                var position = _transformSystem.GetGridOrMapTilePosition(match, xform);

                _atmosphereSystem.HotspotExpose(gridUid, position, 400, 50, match, true);
            }
        }

        private void OnInteractUsing(Entity<MatchstickComponent> ent, ref InteractUsingEvent args)
        {
            if (args.Handled || ent.Comp.CurrentState != SmokableState.Unlit)
                return;

            var isHotEvent = new IsHotEvent();
            RaiseLocalEvent(args.Used, isHotEvent);

            if (!isHotEvent.IsHot)
                return;

            Ignite(ent, args.User);
            args.Handled = true;
        }

        private void OnIsHotEvent(EntityUid uid, MatchstickComponent component, IsHotEvent args)
        {
            args.IsHot = component.CurrentState == SmokableState.Lit;
        }

        public void Ignite(Entity<MatchstickComponent> matchstick, EntityUid user)
        {
            var component = matchstick.Comp;

            // Play Sound
            _audio.PlayPvs(component.IgniteSound, matchstick, AudioParams.Default.WithVariation(0.125f).WithVolume(-0.125f));

            // Change state
            SetState((matchstick, component), SmokableState.Lit); // Shitmed Change
            _litMatches.Add(matchstick);
            matchstick.Owner.SpawnTimer(component.Duration * 1000, delegate
            {
                SetState((matchstick, component), SmokableState.Burnt); // Shitmed Change
                _litMatches.Remove(matchstick);
            });
        }

        // Shitmed Change Start
        public override bool SetState(Entity<MatchstickComponent> ent, SmokableState value)
        {
            if (!base.SetState(ent, value))
                return false;

            var (uid, component) = ent;

        // Shitmed Change End

            if (_lights.TryGetLight(uid, out var pointLightComponent))
            {
                _lights.SetEnabled(uid, component.CurrentState == SmokableState.Lit, pointLightComponent);
            }

            if (EntityManager.TryGetComponent(uid, out ItemComponent? item))
            {
                switch (component.CurrentState)
                {
                    case SmokableState.Lit:
                        _item.SetHeldPrefix(uid, "lit", component: item);
                        break;
                    default:
                        _item.SetHeldPrefix(uid, "unlit", component: item);
                        break;
                }
            }

            if (EntityManager.TryGetComponent(uid, out AppearanceComponent? appearance))
            {
                _appearance.SetData(uid, SmokingVisuals.Smoking, component.CurrentState, appearance);
            }


            return true; // Shitmed Change
        }
    }
}