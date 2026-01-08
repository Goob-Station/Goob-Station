// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Server.Power.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.DoAfter;
using Content.Shared.PowerCell.Components;
using Content.Shared._EinsteinEngines.Silicon;
using Content.Shared.Verbs;
using Robust.Shared.Utility;
using Content.Server._EinsteinEngines.Silicon.Charge;
using Content.Shared._EinsteinEngines.Silicon.Charge; // Goobstation - Energycrit: BatteryDrinkerSourceComponent moved to shared
using Content.Server.Power.EntitySystems;
using Content.Server.Popups;
using Content.Server.PowerCell;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
// Goobstation Start - Energycrit
using Content.Shared._EinsteinEngines.Power.Components;
using Content.Shared._EinsteinEngines.Power.Systems;
using Content.Shared.Whitelist;
// Goobstation End

namespace Content.Server._EinsteinEngines.Power;

// Goobstation - Energycrit: Create SharedBatteryDrinkerSystem and Client BatteryDrinkerSystem so client can predict drink verbs
public sealed class BatteryDrinkerSystem : SharedBatteryDrinkerSystem
{
    [Dependency] private readonly ItemSlotsSystem _slots = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly BatterySystem _battery = default!;
    [Dependency] private readonly SiliconChargeSystem _silicon = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly PowerCellSystem _powerCell = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly ChargerSystem _chargers = default!; // Goobstation
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!; // Goobstation - Energycrit

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BatteryComponent, GetVerbsEvent<AlternativeVerb>>(AddAltVerb);
        SubscribeLocalEvent<PowerCellSlotComponent, GetVerbsEvent<AlternativeVerb>>(AddAltVerb); // Goobstation - Energycrit

        SubscribeLocalEvent<BatteryDrinkerComponent, BatteryDrinkerDoAfterEvent>(OnDoAfter);
    }

    // Goobstation - Energycrit: Switched component from BatteryComponent to generic type.
    private void AddAltVerb<TComp>(EntityUid uid, TComp component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        if (!TryComp<BatteryDrinkerComponent>(args.User, out var drinkerComp) ||
            // Goobstation Start - Energycrit
            _whitelist.IsBlacklistPass(drinkerComp.Blacklist, uid) ||
            !SearchForDrinker(args.User, out _) ||
            !SearchForSource(uid, out var battery) ||
            !TestDrinkableBattery(battery.Value, drinkerComp))
            // Goobstation End - Energycrit
            return;

        AlternativeVerb verb = new()
        {
            // Goobstation - Energycrit
            Act = () => DrinkBattery(battery.Value, args.User, drinkerComp),
            Text = Loc.GetString("battery-drinker-verb-drink"),
            Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/smite.svg.192dpi.png")),
            // Goobstation - Energycrit: dont block removing power cells
            Priority = -5
        };

        args.Verbs.Add(verb);
    }

    private bool TestDrinkableBattery(EntityUid target, BatteryDrinkerComponent drinkerComp)
    {
        // Goobstation - Energycrit: Remove DrinkAll
        if (!HasComp<BatteryDrinkerSourceComponent>(target))
            return false;

        return true;
    }

    private void DrinkBattery(EntityUid target, EntityUid user, BatteryDrinkerComponent drinkerComp)
    {
        var doAfterTime = drinkerComp.DrinkSpeed;

        // Goobstation - Energycrit: Remove DrinkAll
        if (TryComp<BatteryDrinkerSourceComponent>(target, out var sourceComp))
            doAfterTime *= sourceComp.DrinkSpeedMulti;
        else
            return;

        var args = new DoAfterArgs(EntityManager, user, doAfterTime, new BatteryDrinkerDoAfterEvent(), user, target) // TODO: Make this doafter loop, once we merge Upstream.
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            Broadcast = false,
            DistanceThreshold = 1.35f,
            RequireCanInteract = true,
            CancelDuplicate = false,
            MultiplyDelay = false, // Goobstation
        };

        _doAfter.TryStartDoAfter(args);
    }

    private void OnDoAfter(EntityUid uid, BatteryDrinkerComponent drinkerComp, DoAfterEvent args)
    {
        if (args.Cancelled || args.Target == null)
            return;

        var source = args.Target.Value;
        var drinker = uid;
        var sourceBattery = Comp<BatteryComponent>(source);

        // Goobstation - Energycrit
        if (!SearchForDrinker(drinker, out var drinkerBattery) ||
            !TryComp<BatteryComponent>(drinkerBattery, out var drinkerBatteryComponent))
            return;

        TryComp<BatteryDrinkerSourceComponent>(source, out var sourceComp);

        var amountToDrink = drinkerComp.DrinkMultiplier * 1000;

        amountToDrink = MathF.Min(amountToDrink, sourceBattery.CurrentCharge);
        amountToDrink = MathF.Min(amountToDrink, drinkerBatteryComponent!.MaxCharge - drinkerBatteryComponent.CurrentCharge);

        if (sourceComp != null && sourceComp.MaxAmount > 0)
            amountToDrink = MathF.Min(amountToDrink, (float) sourceComp.MaxAmount);

        if (amountToDrink <= 0)
        {
            _popup.PopupEntity(Loc.GetString("battery-drinker-empty", ("target", source)), drinker, drinker);
            return;
        }

        if (_battery.TryUseCharge(source, amountToDrink))
            _battery.SetCharge(drinkerBattery.Value, drinkerBatteryComponent.CurrentCharge + amountToDrink, drinkerBatteryComponent); // DeltaV - people with augment power cells can drink batteries
        else
        {
            _battery.SetCharge(drinkerBattery.Value, sourceBattery.CurrentCharge + drinkerBatteryComponent.CurrentCharge, drinkerBatteryComponent); // DeltaV - people with augment power cells can drink batteries
            _battery.SetCharge(source, 0);
        }

        if (sourceComp != null && sourceComp.DrinkSound != null){
            _popup.PopupEntity(Loc.GetString("ipc-recharge-tip"), drinker, drinker, PopupType.SmallCaution);
            _audio.PlayPvs(sourceComp.DrinkSound, source);
            Spawn("EffectSparks", Transform(source).Coordinates);
        }
    }
}
