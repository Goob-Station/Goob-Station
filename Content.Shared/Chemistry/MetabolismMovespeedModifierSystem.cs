// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2021 ScalyChimp <72841710+scaly-chimp@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2021 mirrorcult <notzombiedude@gmail.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Chemistry.Components;
using Content.Shared.Movement.Systems;
using Robust.Shared.Timing;

namespace Content.Shared.Chemistry
{
    // TODO CONVERT THIS TO A STATUS EFFECT!!!!!!!!!!!!!!!!!!!!!!!!
    public sealed class MetabolismMovespeedModifierSystem : EntitySystem
    {
        [Dependency] private readonly IGameTiming _gameTiming = default!;
        [Dependency] private readonly MovementSpeedModifierSystem _movespeed = default!;

        private readonly List<Entity<MovespeedModifierMetabolismComponent>> _components = new();

        public override void Initialize()
        {
            base.Initialize();

            UpdatesOutsidePrediction = true;

            SubscribeLocalEvent<MovespeedModifierMetabolismComponent, ComponentStartup>(AddComponent);
            SubscribeLocalEvent<MovespeedModifierMetabolismComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovespeed);
        }

        private void OnRefreshMovespeed(EntityUid uid, MovespeedModifierMetabolismComponent component, RefreshMovementSpeedModifiersEvent args)
        {
            args.ModifySpeed(component.WalkSpeedModifier, component.SprintSpeedModifier);
        }

        private void AddComponent(Entity<MovespeedModifierMetabolismComponent> metabolism, ref ComponentStartup args)
        {
            _components.Add(metabolism);
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            var currentTime = _gameTiming.CurTime;

            for (var i = _components.Count - 1; i >= 0; i--)
            {
                var metabolism = _components[i];

                if (metabolism.Comp.Deleted)
                {
                    _components.RemoveAt(i);
                    continue;
                }

                if (metabolism.Comp.ModifierTimer > currentTime)
                    continue;

                _components.RemoveAt(i);
                EntityManager.RemoveComponent<MovespeedModifierMetabolismComponent>(metabolism);

                _movespeed.RefreshMovementSpeedModifiers(metabolism);
            }
        }
    }
}