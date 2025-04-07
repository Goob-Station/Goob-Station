// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 FoLoKe <36813380+FoLoKe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 themias <89101928+themias@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 Saphire Lattice <lattice@saphi.re>
// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 beck-thompson <107373427+beck-thompson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Krunklehorn <42424291+Krunklehorn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Magnus Larsen <i.am.larsenml@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.Containers.ItemSlots;
using Content.Server.Nutrition.Components;
using Content.Server.Popups;
using Content.Shared.Interaction;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Tools.EntitySystems;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;

namespace Content.Server.Nutrition.EntitySystems
{
    /// <summary>
    /// Handles usage of the utensils on the food items
    /// </summary>
    internal sealed class UtensilSystem : SharedUtensilSystem
    {
        [Dependency] private readonly IRobustRandom _robustRandom = default!;
        [Dependency] private readonly FoodSystem _foodSystem = default!;
        [Dependency] private readonly PopupSystem _popupSystem = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;
        [Dependency] private readonly SharedInteractionSystem _interactionSystem = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<UtensilComponent, AfterInteractEvent>(OnAfterInteract, after: new[] { typeof(ItemSlotsSystem), typeof(ToolOpenableSystem) });
        }

        /// <summary>
        /// Clicked with utensil
        /// </summary>
        private void OnAfterInteract(Entity<UtensilComponent> entity, ref AfterInteractEvent ev)
        {
            if (ev.Handled || ev.Target == null || !ev.CanReach)
                return;

            var result = TryUseUtensil(ev.User, ev.Target.Value, entity);
            ev.Handled = result.Handled;
        }

        public (bool Success, bool Handled) TryUseUtensil(EntityUid user, EntityUid target, Entity<UtensilComponent> utensil)
        {
            if (!EntityManager.TryGetComponent(target, out FoodComponent? food))
                return (false, false);

            //Prevents food usage with a wrong utensil
            if ((food.Utensil & utensil.Comp.Types) == 0)
            {
                _popupSystem.PopupEntity(Loc.GetString("food-system-wrong-utensil", ("food", target), ("utensil", utensil.Owner)), user, user);
                return (false, true);
            }

            if (!_interactionSystem.InRangeUnobstructed(user, target, popup: true))
                return (false, true);

            return _foodSystem.TryFeed(user, user, target, food);
        }

        /// <summary>
        /// Attempt to break the utensil after interaction.
        /// </summary>
        /// <param name="uid">Utensil.</param>
        /// <param name="userUid">User of the utensil.</param>
        public void TryBreak(EntityUid uid, EntityUid userUid, UtensilComponent? component = null)
        {
            if (!Resolve(uid, ref component))
                return;

            if (_robustRandom.Prob(component.BreakChance))
            {
                _audio.PlayPvs(component.BreakSound, userUid, AudioParams.Default.WithVolume(-2f));
                EntityManager.DeleteEntity(uid);
            }
        }
    }
}