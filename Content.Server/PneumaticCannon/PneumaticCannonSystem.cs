// SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Kara D <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 Wrexbe <wrexbe@protonmail.com>
// SPDX-FileCopyrightText: 2021 pointer-to-null <91910481+pointer-to-null@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Fishfish458 <47410468+Fishfish458@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Paul <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 fishfish458 <fishfish458>
// SPDX-FileCopyrightText: 2022 ike709 <ike709@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Verm <32827189+Vermidia@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Storage.EntitySystems;
using Content.Server.Stunnable;
using Content.Server.Weapons.Ranged.Systems;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Interaction;
using Content.Shared.PneumaticCannon;
using Content.Shared.StatusEffect;
using Content.Shared.Tools.Systems;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Containers;

namespace Content.Server.PneumaticCannon;

public sealed class PneumaticCannonSystem : SharedPneumaticCannonSystem
{
    [Dependency] private readonly AtmosphereSystem _atmos = default!;
    [Dependency] private readonly GasTankSystem _gasTank = default!;
    [Dependency] private readonly GunSystem _gun = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly ItemSlotsSystem _slots = default!;
    [Dependency] private readonly SharedToolSystem _toolSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PneumaticCannonComponent, InteractUsingEvent>(OnInteractUsing, before: new []{ typeof(StorageSystem) });
        SubscribeLocalEvent<PneumaticCannonComponent, GunShotEvent>(OnShoot);
        SubscribeLocalEvent<PneumaticCannonComponent, ContainerIsInsertingAttemptEvent>(OnContainerInserting);
        SubscribeLocalEvent<PneumaticCannonComponent, GunRefreshModifiersEvent>(OnGunRefreshModifiers);
    }

    private void OnInteractUsing(EntityUid uid, PneumaticCannonComponent component, InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        if (!_toolSystem.HasQuality(args.Used, component.ToolModifyPower))
            return;

        var val = (int) component.Power;
        val = (val + 1) % (int) PneumaticCannonPower.Len;
        component.Power = (PneumaticCannonPower) val;

        Popup.PopupEntity(Loc.GetString("pneumatic-cannon-component-change-power",
            ("power", component.Power.ToString())), uid, args.User);

        component.ProjectileSpeed = GetProjectileSpeedFromPower(component);
        if (TryComp<GunComponent>(uid, out var gun))
            _gun.RefreshModifiers((uid, gun));

        args.Handled = true;
    }

    private void OnContainerInserting(EntityUid uid, PneumaticCannonComponent component, ContainerIsInsertingAttemptEvent args)
    {
        if (args.Container.ID != PneumaticCannonComponent.TankSlotId)
            return;

        if (!TryComp<GasTankComponent>(args.EntityUid, out var gas))
            return;

        // only accept tanks if it uses gas
        if (gas.Air.TotalMoles >= component.GasUsage && component.GasUsage > 0f)
            return;

        args.Cancel();
    }

    private void OnShoot(Entity<PneumaticCannonComponent> cannon, ref GunShotEvent args)
    {
        var (uid, component) = cannon;
        // require a gas tank if it uses gas
        var gas = GetGas(cannon);
        if (gas == null && component.GasUsage > 0f)
            return;

        if (TryComp<StatusEffectsComponent>(args.User, out var status)
            && component.Power == PneumaticCannonPower.High)
        {
            _stun.TryParalyze(args.User, TimeSpan.FromSeconds(component.HighPowerStunTime), true, status);
            Popup.PopupEntity(Loc.GetString("pneumatic-cannon-component-power-stun",
                ("cannon", uid)), cannon, args.User);
        }

        // ignore gas stuff if the cannon doesn't use any
        if (gas == null)
            return;

        // this should always be possible, as we'll eject the gas tank when it no longer is
        var environment = _atmos.GetContainingMixture(cannon.Owner, false, true);
        var removed = _gasTank.RemoveAir(gas.Value, component.GasUsage);
        if (environment != null && removed != null)
        {
            _atmos.Merge(environment, removed);
        }

        if (gas.Value.Comp.Air.TotalMoles >= component.GasUsage)
            return;

        // eject gas tank
        _slots.TryEject(uid, PneumaticCannonComponent.TankSlotId, args.User, out _);
    }

    private void OnGunRefreshModifiers(Entity<PneumaticCannonComponent> ent, ref GunRefreshModifiersEvent args)
    {
        if (ent.Comp.ProjectileSpeed is { } speed)
            args.ProjectileSpeed = speed;
    }

    /// <summary>
    ///     Returns whether the pneumatic cannon has enough gas to shoot an item, as well as the tank itself.
    /// </summary>
    private Entity<GasTankComponent>? GetGas(EntityUid uid)
    {
        if (!Container.TryGetContainer(uid, PneumaticCannonComponent.TankSlotId, out var container) ||
            container is not ContainerSlot slot || slot.ContainedEntity is not {} contained)
            return null;

        return TryComp<GasTankComponent>(contained, out var gasTank) ? (contained, gasTank) : null;
    }

    private float GetProjectileSpeedFromPower(PneumaticCannonComponent component)
    {
        return component.Power switch
        {
            PneumaticCannonPower.High => component.BaseProjectileSpeed * 4f,
            PneumaticCannonPower.Medium => component.BaseProjectileSpeed,
            PneumaticCannonPower.Low or _ => component.BaseProjectileSpeed * 0.5f,
        };
    }
}