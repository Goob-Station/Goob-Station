// SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Kara D <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2021 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2022 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2022 Julian Giebel <juliangiebel@live.de>
// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Menshin <Menshin@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 BombasterDS <deniskaporoshok@gmail.com>
// SPDX-FileCopyrightText: 2025 BombasterDS2 <shvalovdenis.workmail@gmail.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using Content.Server._Goobstation.Wizard.Systems;
using Content.Server.Administration.Logs;
using Content.Server.Atmos.Components;
using Content.Shared._Goobstation.Wizard.Spellblade;
using Content.Shared.Alert;
using Content.Shared.Atmos;
using Content.Shared.Damage;
using Content.Shared.Database;
using Content.Shared.FixedPoint;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Robust.Shared.Containers;

namespace Content.Server.Atmos.EntitySystems
{
    public sealed class BarotraumaSystem : EntitySystem
    {
        [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
        [Dependency] private readonly DamageableSystem _damageableSystem = default!;
        [Dependency] private readonly AlertsSystem _alertsSystem = default!;
        [Dependency] private readonly IAdminLogManager _adminLogger= default!;
        [Dependency] private readonly InventorySystem _inventorySystem = default!;
        [Dependency] private readonly SpellbladeSystem _spellblade = default!; // Goobstation

        private const float UpdateTimer = 1f;
        private float _timer;

        public override void Initialize()
        {
            SubscribeLocalEvent<PressureProtectionComponent, GotEquippedEvent>(OnPressureProtectionEquipped);
            SubscribeLocalEvent<PressureProtectionComponent, GotUnequippedEvent>(OnPressureProtectionUnequipped);
            SubscribeLocalEvent<PressureProtectionComponent, ComponentInit>(OnPressureProtectionChanged); // Goobstation - Update component state on toggle
            SubscribeLocalEvent<PressureProtectionComponent, ComponentRemove>(OnPressureProtectionChanged); // Goobstation - Update component state on toggle

            SubscribeLocalEvent<PressureImmunityComponent, ComponentInit>(OnPressureImmuneInit);
            SubscribeLocalEvent<PressureImmunityComponent, ComponentRemove>(OnPressureImmuneRemove);
        }

        private void OnPressureImmuneInit(EntityUid uid, PressureImmunityComponent pressureImmunity, ComponentInit args)
        {
            if (TryComp<BarotraumaComponent>(uid, out var barotrauma))
            {
                barotrauma.HasImmunity = true;
            }
        }

        private void OnPressureImmuneRemove(EntityUid uid, PressureImmunityComponent pressureImmunity, ComponentRemove args)
        {
            if (TryComp<BarotraumaComponent>(uid, out var barotrauma))
            {
                barotrauma.HasImmunity = false;
            }
        }

        // Goobstation - Modsuits - Update component state on toggle
        private void OnPressureProtectionChanged(EntityUid uid, PressureProtectionComponent pressureProtection, EntityEventArgs args)
        {
            var protectionTarget = uid;
            string? slotTarget = null;

            if (_inventorySystem.TryGetContainingEntity(uid, out var entity) && _inventorySystem.TryGetContainingSlot(uid, out var slot))
            {
                protectionTarget = entity.Value;
                slotTarget = slot.Name;
            }

            if (TryComp<BarotraumaComponent>(protectionTarget, out var barotrauma))
            {
                if (slotTarget != null && !barotrauma.ProtectionSlots.Contains(slotTarget))
                    return;

                UpdateCachedResistances(protectionTarget, barotrauma);
            }
        }

        /// <summary>
        /// Generic method for updating resistance on component Lifestage events
        /// </summary>
        private void OnUpdateResistance(EntityUid uid, PressureProtectionComponent pressureProtection, EntityEventArgs args)
        {
            if (TryComp<BarotraumaComponent>(uid, out var barotrauma))
            {
                UpdateCachedResistances(uid, barotrauma);
            }
        }

        private void OnPressureProtectionEquipped(EntityUid uid, PressureProtectionComponent pressureProtection, GotEquippedEvent args)
        {
            if (TryComp<BarotraumaComponent>(args.Equipee, out var barotrauma) && barotrauma.ProtectionSlots.Contains(args.Slot))
            {
                UpdateCachedResistances(args.Equipee, barotrauma);
            }
        }

        private void OnPressureProtectionUnequipped(EntityUid uid, PressureProtectionComponent pressureProtection, GotUnequippedEvent args)
        {
            if (TryComp<BarotraumaComponent>(args.Equipee, out var barotrauma) && barotrauma.ProtectionSlots.Contains(args.Slot))
            {
                UpdateCachedResistances(args.Equipee, barotrauma);
            }
        }

        /// <summary>
        /// Computes the pressure resistance for the entity coming from the equipment and any innate resistance.
        /// The ProtectionSlots field of the Barotrauma component specifies which parts must be protected for the protection to have any effet.
        /// </summary>
        private void UpdateCachedResistances(EntityUid uid, BarotraumaComponent barotrauma)
        {

            if (barotrauma.ProtectionSlots.Count != 0)
            {
                if (!TryComp(uid, out InventoryComponent? inv) || !TryComp(uid, out ContainerManagerComponent? contMan))
                {
                    return;
                }
                var hPModifier = float.MinValue;
                var hPMultiplier = float.MinValue;
                var lPModifier = float.MaxValue;
                var lPMultiplier = float.MaxValue;

                foreach (var slot in barotrauma.ProtectionSlots)
                {
                    if (!_inventorySystem.TryGetSlotEntity(uid, slot, out var equipment, inv, contMan)
                        || !TryGetPressureProtectionValues(equipment.Value,
                            out var itemHighMultiplier,
                            out var itemHighModifier,
                            out var itemLowMultiplier,
                            out var itemLowModifier))
                    {
                        // Missing protection, skin is exposed.
                        hPModifier = 0f;
                        hPMultiplier = 1f;
                        lPModifier = 0f;
                        lPMultiplier = 1f;
                        break;
                    }

                    // The entity is as protected as its weakest part protection
                    hPModifier = Math.Max(hPModifier, itemHighModifier.Value);
                    hPMultiplier = Math.Max(hPMultiplier, itemHighMultiplier.Value);
                    lPModifier = Math.Min(lPModifier, itemLowModifier.Value);
                    lPMultiplier = Math.Min(lPMultiplier, itemLowMultiplier.Value);
                }

                barotrauma.HighPressureModifier = hPModifier;
                barotrauma.HighPressureMultiplier = hPMultiplier;
                barotrauma.LowPressureModifier = lPModifier;
                barotrauma.LowPressureMultiplier = lPMultiplier;
            }

            // any innate pressure resistance ?
            if (TryGetPressureProtectionValues(uid,
                    out var highMultiplier,
                    out var highModifier,
                    out var lowMultiplier,
                    out var lowModifier))
            {
                barotrauma.HighPressureModifier += highModifier.Value;
                barotrauma.HighPressureMultiplier *= highMultiplier.Value;
                barotrauma.LowPressureModifier += lowModifier.Value;
                barotrauma.LowPressureMultiplier *= lowMultiplier.Value;
            }
        }

        /// <summary>
        /// Returns adjusted pressure after having applied resistances from equipment and innate (if any), to check against a low pressure hazard threshold
        /// </summary>
        public float GetFeltLowPressure(EntityUid uid, BarotraumaComponent barotrauma, float environmentPressure)
        {
            if (barotrauma.HasImmunity)
            {
                return Atmospherics.OneAtmosphere;
            }

            var modified = (environmentPressure + barotrauma.LowPressureModifier) * (barotrauma.LowPressureMultiplier);
            return Math.Min(modified, Atmospherics.OneAtmosphere);
        }

        /// <summary>
        /// Returns adjusted pressure after having applied resistances from equipment and innate (if any), to check against a high pressure hazard threshold
        /// </summary>
        public float GetFeltHighPressure(EntityUid uid, BarotraumaComponent barotrauma, float environmentPressure)
        {
            if (barotrauma.HasImmunity)
            {
                return Atmospherics.OneAtmosphere;
            }

            var modified = (environmentPressure + barotrauma.HighPressureModifier) * (barotrauma.HighPressureMultiplier);
            return Math.Max(modified, Atmospherics.OneAtmosphere);
        }

        public bool TryGetPressureProtectionValues(
            EntityUid ent, // Goob edit
            [NotNullWhen(true)] out float? highMultiplier,
            [NotNullWhen(true)] out float? highModifier,
            [NotNullWhen(true)] out float? lowMultiplier,
            [NotNullWhen(true)] out float? lowModifier)
        {
            highMultiplier = null;
            highModifier = null;
            lowMultiplier = null;
            lowModifier = null;

            // Goob edit start
            var ev = new GetPressureProtectionValuesEvent
            {
                HighPressureMultiplier = 1f,
                HighPressureModifier = 0f,
                LowPressureMultiplier = 1f,
                LowPressureModifier = 0f
            };

            if (TryComp(ent, out PressureProtectionComponent? comp))
            {
                ev.HighPressureMultiplier = comp.HighPressureMultiplier;
                ev.HighPressureModifier = comp.HighPressureModifier;
                ev.LowPressureMultiplier = comp.LowPressureMultiplier;
                ev.LowPressureModifier = comp.LowPressureModifier;
            }
            // Goob edit end

            RaiseLocalEvent(ent, ref ev);
            highMultiplier = ev.HighPressureMultiplier;
            highModifier = ev.HighPressureModifier;
            lowMultiplier = ev.LowPressureMultiplier;
            lowModifier = ev.LowPressureModifier;
            return true;
        }

        public override void Update(float frameTime)
        {
            _timer += frameTime;

            if (_timer < UpdateTimer)
                return;

            _timer -= UpdateTimer;

            var enumerator = EntityQueryEnumerator<BarotraumaComponent, DamageableComponent>();
            while (enumerator.MoveNext(out var uid, out var barotrauma, out var damageable))
            {
                var totalDamage = FixedPoint2.Zero;
                foreach (var (barotraumaDamageType, _) in barotrauma.Damage.DamageDict)
                {
                    if (!damageable.Damage.DamageDict.TryGetValue(barotraumaDamageType, out var damage))
                        continue;
                    totalDamage += damage;
                }
                if (totalDamage >= barotrauma.MaxDamage)
                    continue;

                var pressure = 1f;

                if (_atmosphereSystem.GetContainingMixture(uid) is {} mixture)
                {
                    pressure = MathF.Max(mixture.Pressure, 1f);
                }

                pressure = pressure switch
                {
                    // Adjust pressure based on equipment. Works differently depending on if it's "high" or "low".
                    <= Atmospherics.WarningLowPressure => GetFeltLowPressure(uid, barotrauma, pressure),
                    >= Atmospherics.WarningHighPressure => GetFeltHighPressure(uid, barotrauma, pressure),
                    _ => pressure
                };

                if (pressure <= Atmospherics.HazardLowPressure)
                {
                    // Deal damage and ignore resistances. Resistance to pressure damage should be done via pressure protection gear.
                    _damageableSystem.TryChangeDamage(uid, barotrauma.Damage * Atmospherics.LowPressureDamage, true, false, canSever: false, partMultiplier: 0.2f); // Shitmed Change

                    if (!barotrauma.TakingDamage)
                    {
                        barotrauma.TakingDamage = true;
                        _adminLogger.Add(LogType.Barotrauma, $"{ToPrettyString(uid):entity} started taking low pressure damage");
                    }

                    _alertsSystem.ShowAlert(uid, barotrauma.LowPressureAlert, 2);
                }
                else if (pressure >= Atmospherics.HazardHighPressure && !_spellblade.IsHoldingItemWithComponent<FireSpellbladeEnchantmentComponent>(uid)) // Goob edit
                {
                    var damageScale = MathF.Min(((pressure / Atmospherics.HazardHighPressure) - 1) * Atmospherics.PressureDamageCoefficient, Atmospherics.MaxHighPressureDamage);

                    // Deal damage and ignore resistances. Resistance to pressure damage should be done via pressure protection gear.
                    _damageableSystem.TryChangeDamage(uid, barotrauma.Damage * damageScale, true, false, canSever: false); // Shitmed Change

                    if (!barotrauma.TakingDamage)
                    {
                        barotrauma.TakingDamage = true;
                        _adminLogger.Add(LogType.Barotrauma, $"{ToPrettyString(uid):entity} started taking high pressure damage");
                    }

                    _alertsSystem.ShowAlert(uid, barotrauma.HighPressureAlert, 2);
                }
                else
                {
                    // Within safe pressure limits
                    if (barotrauma.TakingDamage)
                    {
                        barotrauma.TakingDamage = false;
                        _adminLogger.Add(LogType.Barotrauma, $"{ToPrettyString(uid):entity} stopped taking pressure damage");
                    }

                    // Set correct alert.
                    switch (pressure)
                    {
                        case <= Atmospherics.WarningLowPressure:
                            _alertsSystem.ShowAlert(uid, barotrauma.LowPressureAlert, 1);
                            break;
                        case >= Atmospherics.WarningHighPressure:
                            _alertsSystem.ShowAlert(uid, barotrauma.HighPressureAlert, 1);
                            break;
                        default:
                            _alertsSystem.ClearAlertCategory(uid, barotrauma.PressureAlertCategory);
                            break;
                    }
                }
            }
        }
    }
}