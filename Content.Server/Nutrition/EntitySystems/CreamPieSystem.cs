// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 Ygg01 <y.laughing.man.y@gmail.com>
// SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Jacob Tong <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Khoa Nguyen <akhoa.nv@gmail.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Cojoke <83733158+Cojoke-dot@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Winkarst <74284083+Winkarst-cpu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Vyacheslav Kovalevsky <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aexxie <codyfox.077@gmail.com>
// SPDX-FileCopyrightText: 2025 pathetic meowmeow <uhhadd@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Server.Explosion.EntitySystems;
using Content.Server.Fluids.EntitySystems;
using Content.Server.Nutrition.Components;
using Content.Server.Popups;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Explosion.Components;
using Content.Shared.IdentityManagement;
using Content.Shared.Nutrition;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Rejuvenate;
using Content.Shared.Throwing;
using Content.Shared.Chemistry.EntitySystems;
using JetBrains.Annotations;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;

namespace Content.Server.Nutrition.EntitySystems
{
    [UsedImplicitly]
    public sealed class CreamPieSystem : SharedCreamPieSystem
    {
        [Dependency] private readonly SharedSolutionContainerSystem _solutions = default!;
        [Dependency] private readonly PuddleSystem _puddle = default!;
        [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
        [Dependency] private readonly TriggerSystem _trigger = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;
        [Dependency] private readonly PopupSystem _popup = default!;

        public override void Initialize()
        {
            base.Initialize();

            // activate BEFORE entity is deleted and trash is spawned
            SubscribeLocalEvent<CreamPieComponent, ConsumeDoAfterEvent>(OnConsume, before: [typeof(FoodSystem)]);
            SubscribeLocalEvent<CreamPieComponent, SliceFoodEvent>(OnSlice);

            SubscribeLocalEvent<CreamPiedComponent, RejuvenateEvent>(OnRejuvenate);
        }

        protected override void SplattedCreamPie(EntityUid uid, CreamPieComponent creamPie)
        {
            // The entity is deleted, so play the sound at its position rather than parenting
            var coordinates = Transform(uid).Coordinates;
            _audio.PlayPvs(_audio.ResolveSound(creamPie.Sound), coordinates, AudioParams.Default.WithVariation(0.125f));

            if (EntityManager.TryGetComponent(uid, out FoodComponent? foodComp))
            {
                if (_solutions.TryGetSolution(uid, foodComp.Solution, out _, out var solution))
                {
                    _puddle.TrySpillAt(uid, solution, out _, false);
                }
                foreach (var trash in foodComp.Trash)
                {
                    EntityManager.SpawnEntity(trash, Transform(uid).Coordinates);
                }
            }
            ActivatePayload(uid);

            EntityManager.QueueDeleteEntity(uid);
        }

        private void OnConsume(Entity<CreamPieComponent> entity, ref ConsumeDoAfterEvent args)
        {
            ActivatePayload(entity);
        }

        private void OnSlice(Entity<CreamPieComponent> entity, ref SliceFoodEvent args)
        {
            ActivatePayload(entity);
        }

        private void ActivatePayload(EntityUid uid)
        {
            if (_itemSlots.TryGetSlot(uid, CreamPieComponent.PayloadSlotName, out var itemSlot))
            {
                if (_itemSlots.TryEject(uid, itemSlot, user: null, out var item))
                {
                    if (TryComp<OnUseTimerTriggerComponent>(item.Value, out var timerTrigger))
                    {
                        _trigger.HandleTimerTrigger(
                            item.Value,
                            null,
                            timerTrigger.Delay,
                            timerTrigger.BeepInterval,
                            timerTrigger.InitialBeepDelay,
                            timerTrigger.BeepSound);
                    }
                }
            }
        }

        protected override void CreamedEntity(EntityUid uid, CreamPiedComponent creamPied, ThrowHitByEvent args)
        {
            _popup.PopupEntity(Loc.GetString("cream-pied-component-on-hit-by-message", ("thrower", args.Thrown)), uid, args.Target);
            var otherPlayers = Filter.Empty().AddPlayersByPvs(uid);
            if (TryComp<ActorComponent>(args.Target, out var actor))
            {
                otherPlayers.RemovePlayer(actor.PlayerSession);
            }
            _popup.PopupEntity(Loc.GetString("cream-pied-component-on-hit-by-message-others", ("owner", Identity.Name(uid, EntityManager)), ("thrower", args.Thrown)), uid, otherPlayers, false);
        }

        private void OnRejuvenate(Entity<CreamPiedComponent> entity, ref RejuvenateEvent args)
        {
            SetCreamPied(entity, entity.Comp, false);
        }
    }
}