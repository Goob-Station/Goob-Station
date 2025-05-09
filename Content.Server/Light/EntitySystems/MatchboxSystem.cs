// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1+git@googlemail.com>
// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DamianX <DamianX@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Light.Components;
using Content.Server.Storage.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Smoking;
using Content.Shared.Smoking.Components; // Shitmed Change

namespace Content.Server.Light.EntitySystems
{
    public sealed class MatchboxSystem : EntitySystem
    {
        [Dependency] private readonly MatchstickSystem _stickSystem = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<MatchboxComponent, InteractUsingEvent>(OnInteractUsing, before: new[] { typeof(StorageSystem) });
        }

        private void OnInteractUsing(EntityUid uid, MatchboxComponent component, InteractUsingEvent args)
        {
            if (!args.Handled
                && EntityManager.TryGetComponent(args.Used, out MatchstickComponent? matchstick)
                && matchstick.CurrentState == SmokableState.Unlit)
            {
                _stickSystem.Ignite((args.Used, matchstick), args.User);
                args.Handled = true;
            }
        }
    }
}