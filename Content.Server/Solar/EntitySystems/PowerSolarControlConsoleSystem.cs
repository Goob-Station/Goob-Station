// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 collinlunn <60152240+collinlunn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Solar.Components;
using Content.Shared.Solar;
using JetBrains.Annotations;
using Robust.Server.GameObjects;

namespace Content.Server.Solar.EntitySystems
{
    /// <summary>
    /// Responsible for updating solar control consoles.
    /// </summary>
    [UsedImplicitly]
    internal sealed class PowerSolarControlConsoleSystem : EntitySystem
    {
        [Dependency] private readonly PowerSolarSystem _powerSolarSystem = default!;
        [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;

        /// <summary>
        /// Timer used to avoid updating the UI state every frame (which would be overkill)
        /// </summary>
        private float _updateTimer;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<SolarControlConsoleComponent, SolarControlConsoleAdjustMessage>(OnUIMessage);
        }

        public override void Update(float frameTime)
        {
            _updateTimer += frameTime;
            if (_updateTimer >= 1)
            {
                _updateTimer -= 1;
                var state = new SolarControlConsoleBoundInterfaceState(_powerSolarSystem.TargetPanelRotation, _powerSolarSystem.TargetPanelVelocity, _powerSolarSystem.TotalPanelPower, _powerSolarSystem.TowardsSun);
                var query = EntityQueryEnumerator<SolarControlConsoleComponent, UserInterfaceComponent>();
                while (query.MoveNext(out var uid, out _, out var uiComp))
                {
                    _uiSystem.SetUiState((uid, uiComp), SolarControlConsoleUiKey.Key, state);
                }
            }
        }

        private void OnUIMessage(EntityUid uid, SolarControlConsoleComponent component, SolarControlConsoleAdjustMessage msg)
        {
            if (double.IsFinite(msg.Rotation))
            {
                _powerSolarSystem.TargetPanelRotation = msg.Rotation.Reduced();
            }
            if (double.IsFinite(msg.AngularVelocity))
            {
                var degrees = msg.AngularVelocity.Degrees;
                degrees = Math.Clamp(degrees, -PowerSolarSystem.MaxPanelVelocityDegrees, PowerSolarSystem.MaxPanelVelocityDegrees);
                _powerSolarSystem.TargetPanelVelocity = Angle.FromDegrees(degrees);
            }
        }

    }
}