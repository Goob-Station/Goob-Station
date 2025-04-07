// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2021 Ygg01 <y.laughing.man.y@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 Kara D <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1+git@googlemail.com>
// SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Arendian <137322659+Arendian@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Zachary Higgs <compgeek223@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Server.Fluids.EntitySystems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reaction;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Slippery;
using Content.Shared.StepTrigger.Components;
using Content.Shared.StepTrigger.Systems;
using JetBrains.Annotations;
using Robust.Shared.Map;

namespace Content.Server.Chemistry.TileReactions
{
    [UsedImplicitly]
    [DataDefinition]
    public sealed partial class SpillTileReaction : ITileReaction
    {
        [DataField("launchForwardsMultiplier")] public float LaunchForwardsMultiplier = 1;
        [DataField("requiredSlipSpeed")] public float RequiredSlipSpeed = 6;
        [DataField("paralyzeTime")] public float ParalyzeTime = 1;

        /// <summary>
        /// <see cref="SlipperyComponent.SuperSlippery"/>
        /// </summary>
        [DataField("superSlippery")] public bool SuperSlippery;

        public FixedPoint2 TileReact(TileRef tile,
            ReagentPrototype reagent,
            FixedPoint2 reactVolume,
            IEntityManager entityManager,
            List<ReagentData>? data)
        {
            if (reactVolume < 5)
                return FixedPoint2.Zero;

            if (entityManager.EntitySysManager.GetEntitySystem<PuddleSystem>()
                .TrySpillAt(tile, new Solution(reagent.ID, reactVolume, data), out var puddleUid, false, false))
            {
                var slippery = entityManager.EnsureComponent<SlipperyComponent>(puddleUid);
                slippery.LaunchForwardsMultiplier = LaunchForwardsMultiplier;
                slippery.ParalyzeTime = ParalyzeTime;
                slippery.SuperSlippery = SuperSlippery;
                entityManager.Dirty(puddleUid, slippery);

                var step = entityManager.EnsureComponent<StepTriggerComponent>(puddleUid);
                entityManager.EntitySysManager.GetEntitySystem<StepTriggerSystem>().SetRequiredTriggerSpeed(puddleUid, RequiredSlipSpeed, step);

                var slow = entityManager.EnsureComponent<SpeedModifierContactsComponent>(puddleUid);
                var speedModifier = 1 - reagent.Viscosity;
                entityManager.EntitySysManager.GetEntitySystem<SpeedModifierContactsSystem>().ChangeModifiers(puddleUid, speedModifier, slow);

                return reactVolume;
            }

            return FixedPoint2.Zero;
        }
    }
}