// SPDX-FileCopyrightText: 2021 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Server.Light.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.Power;

namespace Content.Server.Light.EntitySystems
{
    public sealed class LitOnPoweredSystem : EntitySystem
    {
        [Dependency] private readonly SharedPointLightSystem _lights = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<LitOnPoweredComponent, PowerChangedEvent>(OnPowerChanged);
            SubscribeLocalEvent<LitOnPoweredComponent, PowerNetBatterySupplyEvent>(OnPowerSupply);
        }

        private void OnPowerChanged(EntityUid uid, LitOnPoweredComponent component, ref PowerChangedEvent args)
        {
            if (_lights.TryGetLight(uid, out var light))
            {
                _lights.SetEnabled(uid, args.Powered, light);
            }
        }

        private void OnPowerSupply(EntityUid uid, LitOnPoweredComponent component, ref PowerNetBatterySupplyEvent args)
        {
            if (_lights.TryGetLight(uid, out var light))
            {
                _lights.SetEnabled(uid, args.Supply, light);
            }
        }
    }
}