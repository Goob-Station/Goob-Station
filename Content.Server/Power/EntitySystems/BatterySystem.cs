// SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 corentt <36075110+corentt@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Vasilis <vasilis@pikachu.systems>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 BramvanZijp <56019239+BramvanZijp@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Brandon Hu <103440971+Brandon-Huu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Spatison <137375981+Spatison@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 yglop <95057024+yglop@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Errant <35878406+Errant-4@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
// SPDX-FileCopyrightText: 2025 Kirus59 <145689588+Kirus59@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Kyle Tyo <36606155+VerinSenpai@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using Content.Server._White.Blocking;
using Content.Server.Cargo.Systems;
using Content.Server.Emp;
using Content.Server.Power.Components;
using Content.Shared.Emp; // Goobstation
using Content.Shared.Examine;
using Content.Shared.Power;
using Content.Shared.Power.Components;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Rejuvenate;
using JetBrains.Annotations;
using Robust.Shared.Containers;
using Robust.Shared.Utility;
using Robust.Shared.Timing;

namespace Content.Server.Power.EntitySystems;

/// <summary>
/// Responsible for <see cref="BatteryComponent"/>.
/// Unpredicted equivalent of <see cref="PredictedBatterySystem"/>.
/// If you make changes to this make sure to keep the two consistent.
/// </summary>
[UsedImplicitly]
public sealed partial class BatterySystem : SharedBatterySystem
{
    [Dependency] private readonly SharedContainerSystem _containers = default!; // WD EDIT

    [Dependency] private readonly IGameTiming _timing = default!;

    private EntityQuery<EmpDisabledComponent> _disabledQuery; // Goobstation

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BatteryComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<BatteryComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<BatteryComponent, RejuvenateEvent>(OnBatteryRejuvenate);
        SubscribeLocalEvent<PowerNetworkBatteryComponent, RejuvenateEvent>(OnNetBatteryRejuvenate);
        SubscribeLocalEvent<BatteryComponent, PriceCalculationEvent>(CalculateBatteryPrice);
        SubscribeLocalEvent<BatteryComponent, ChangeChargeEvent>(OnChangeCharge);
        SubscribeLocalEvent<BatteryComponent, GetChargeEvent>(OnGetCharge);

        SubscribeLocalEvent<NetworkBatteryPreSync>(PreSync);
        SubscribeLocalEvent<NetworkBatteryPostSync>(PostSync);

        _disabledQuery = GetEntityQuery<EmpDisabledComponent>(); // Goobstation
    }

    private void OnInit(Entity<BatteryComponent> ent, ref ComponentInit args)
    {
        DebugTools.Assert(!HasComp<PredictedBatteryComponent>(ent), $"{ent} has both BatteryComponent and PredictedBatteryComponent");
    }
    private void OnNetBatteryRejuvenate(Entity<PowerNetworkBatteryComponent> ent, ref RejuvenateEvent args)
    {
        ent.Comp.NetworkBattery.CurrentStorage = ent.Comp.NetworkBattery.Capacity;
    }
    private void OnBatteryRejuvenate(Entity<BatteryComponent> ent, ref RejuvenateEvent args)
    {
        SetCharge(ent.AsNullable(), ent.Comp.MaxCharge);
    }

    private void OnExamine(Entity<BatteryComponent> ent, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        if (!HasComp<ExaminableBatteryComponent>(ent))
            return;

        var chargePercentRounded = 0;
        if (ent.Comp.MaxCharge != 0)
            chargePercentRounded = (int)(100 * ent.Comp.CurrentCharge / ent.Comp.MaxCharge);

        args.PushMarkup(
            Loc.GetString(
                "examinable-battery-component-examine-detail",
                ("percent", chargePercentRounded),
                ("markupPercentColor", "green")
            )
        );
    }

    private void PreSync(NetworkBatteryPreSync ev)
    {
        // Ignoring entity pausing. If the entity was paused, neither component's data should have been changed.
        var enumerator = AllEntityQuery<PowerNetworkBatteryComponent, BatteryComponent>();
        while (enumerator.MoveNext(out var netBat, out var bat))
        {
            DebugTools.Assert(bat.CurrentCharge <= bat.MaxCharge && bat.CurrentCharge >= 0);
            netBat.NetworkBattery.Capacity = bat.MaxCharge;
            netBat.NetworkBattery.CurrentStorage = bat.CurrentCharge;
        }
    }

    private void PostSync(NetworkBatteryPostSync ev)
    {
        // Ignoring entity pausing. If the entity was paused, neither component's data should have been changed.
        var enumerator = AllEntityQuery<PowerNetworkBatteryComponent, BatteryComponent>();
        while (enumerator.MoveNext(out var uid, out var netBat, out var bat))
        {
            SetCharge((uid, bat), netBat.NetworkBattery.CurrentStorage);
        }
    }

    /// <summary>
    /// Gets the price for the power contained in an entity's battery.
    /// </summary>
    private void CalculateBatteryPrice(Entity<BatteryComponent> ent, ref PriceCalculationEvent args)
    {
        args.Price += ent.Comp.CurrentCharge * ent.Comp.PricePerJoule;
    }

    private void OnChangeCharge(Entity<BatteryComponent> ent, ref ChangeChargeEvent args)
    {
        if (args.ResidualValue == 0)
            return;

        args.ResidualValue -= ChangeCharge(ent.AsNullable(), args.ResidualValue);
    }

    private void OnGetCharge(Entity<BatteryComponent> entity, ref GetChargeEvent args)
    {
        args.CurrentCharge += entity.Comp.CurrentCharge;
        args.MaxCharge += entity.Comp.MaxCharge;
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<BatterySelfRechargerComponent, BatteryComponent>();
        var curTime = _timing.CurTime;
        while (query.MoveNext(out var uid, out var comp, out var bat))
        {
            if (!comp.AutoRecharge || IsFull((uid, bat)))
                continue;

            if (comp.NextAutoRecharge > curTime)
                continue;

            // Goobstation
            if (comp.CanEmp && _disabledQuery.HasComponent(uid))
                continue;

            SetCharge((uid, bat), bat.CurrentCharge + comp.AutoRechargeRate * frameTime);
        }
    }

    // Goobstation
    public int GetChargeDifference(EntityUid uid, BatteryComponent? battery = null) // Debug
    {
        if (!Resolve(uid, ref battery))
            return 0;

        return Convert.ToInt32(battery.MaxCharge - battery.CurrentCharge);
    }
    public float AddCharge(EntityUid uid, float value, BatteryComponent? battery = null)
    {
        if (value <= 0 || !Resolve(uid, ref battery))
            return 0;

        var newValue = Math.Clamp(battery.CurrentCharge + value, 0, battery.MaxCharge);
        battery.CurrentCharge = newValue;
        var ev = new ChargeChangedEvent(battery.CurrentCharge, battery.MaxCharge);
        RaiseLocalEvent(uid, ref ev);
        return newValue;
    }

    // WD EDIT START
    public bool TryGetBatteryComponent(EntityUid uid, [NotNullWhen(true)] out BatteryComponent? battery,
        [NotNullWhen(true)] out EntityUid? batteryUid)
    {
        if (TryComp(uid, out battery))
        {
            batteryUid = uid;
            return true;
        }

        if (!_containers.TryGetContainer(uid, "cell_slot", out var container)
            || container is not ContainerSlot slot)
        {
            battery = null;
            batteryUid = null;
            return false;
        }

        batteryUid = slot.ContainedEntity;

        if (batteryUid != null)
            return TryComp(batteryUid, out battery);

        battery = null;
        return false;
    }
    // WD EDIT END
}
