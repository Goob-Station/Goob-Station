// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 ScyronX <166930367+ScyronX@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SX-7 <92227810+SX-7@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Tools;
using Content.Shared.Abilities.Oni;
using Content.Shared.Tools.Components;
using Content.Shared.Damage.Events;
using Content.Shared.Item;
using Content.Shared.Nyanotrasen.Abilities.Oni;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Wieldable.Components;
using Robust.Shared.Containers;

namespace Content.Server.Abilities.Oni
{
    public sealed class OniSystem : SharedOniSystem
    {
        [Dependency] private readonly ToolSystem _toolSystem = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<OniComponent, EntInsertedIntoContainerMessage>(OnEntInserted);
            SubscribeLocalEvent<OniComponent, EntRemovedFromContainerMessage>(OnEntRemoved);
            SubscribeLocalEvent<OniComponent, MeleeHitEvent>(OnOniMeleeHit);
            SubscribeLocalEvent<HeldByOniComponent, MeleeHitEvent>(OnHeldMeleeHit);
            SubscribeLocalEvent<HeldByOniComponent, StaminaMeleeHitEvent>(OnStamHit);
        }

        private void OnEntInserted(EntityUid uid, OniComponent component, EntInsertedIntoContainerMessage args)
        {
            var heldComp = EnsureComp<HeldByOniComponent>(args.Entity);
            heldComp.Holder = uid;

            if (TryComp<ToolComponent>(args.Entity, out var tool) && _toolSystem.HasQuality(args.Entity, "Prying", tool))
                tool.SpeedModifier *= 1.66f;

            if (HasComp<GunComponent>(args.Entity)
            && !HasComp<WieldableComponent>(args.Entity)
            && !HasComp<MultiHandedItemComponent>(args.Entity)
            && !HasComp<OniAllowedGunComponent>(args.Entity))
            {
                heldComp.WasOneHanded = true;
                var wieldableComp = EnsureComp<WieldableComponent>(args.Entity);
                wieldableComp.UnwieldOnUse = false;
                wieldableComp.WieldedInhandPrefix = null;
                EnsureComp<GunRequiresWieldComponent>(args.Entity);
            }
        }

        private void OnEntRemoved(EntityUid uid, OniComponent component, EntRemovedFromContainerMessage args)
        {
            if (TryComp<ToolComponent>(args.Entity, out var tool) && _toolSystem.HasQuality(args.Entity, "Prying", tool))
                tool.SpeedModifier /= 1.66f;

            if (HasComp<GunComponent>(args.Entity)
            && TryComp<HeldByOniComponent>(args.Entity, out var heldComp)
            && heldComp.WasOneHanded)
            {
                RemComp<WieldableComponent>(args.Entity);
                RemComp<GunRequiresWieldComponent>(args.Entity);
            }

            RemComp<HeldByOniComponent>(args.Entity);
        }

        private void OnOniMeleeHit(EntityUid uid, OniComponent component, MeleeHitEvent args)
        {
            args.ModifiersList.Add(component.MeleeModifiers);
        }

        private void OnHeldMeleeHit(EntityUid uid, HeldByOniComponent component, MeleeHitEvent args)
        {
            if (!TryComp<OniComponent>(component.Holder, out var oni))
                return;

            args.ModifiersList.Add(oni.MeleeModifiers);
        }

        private void OnStamHit(EntityUid uid, HeldByOniComponent component, StaminaMeleeHitEvent args)
        {
            if (!TryComp<OniComponent>(component.Holder, out var oni))
                return;

            args.Multiplier *= oni.StamDamageMultiplier;
        }
    }
}
