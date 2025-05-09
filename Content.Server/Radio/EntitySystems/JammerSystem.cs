// SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Alice "Arimah" Heurlin <30327355+arimah@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Errant <35878406+Errant-4@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Flareguy <78941145+Flareguy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 HS <81934438+HolySSSS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 IProduceWidgets <107586145+IProduceWidgets@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Mr. 27 <45323883+Dutch-VanDerLinde@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 PJBot <pieterjan.briers+bot@gmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Rouge2t7 <81053047+Sarahon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2024 Truoizys <153248924+Truoizys@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 TsjipTsjip <19798667+TsjipTsjip@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ubaser <134914314+UbaserB@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Vasilis <vasilis@pikachu.systems>
// SPDX-FileCopyrightText: 2024 beck-thompson <107373427+beck-thompson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2024 nikthechampiongr <32041239+nikthechampiongr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 osjarw <62134478+osjarw@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 plykiya <plykiya@protonmail.com>
// SPDX-FileCopyrightText: 2024 Арт <123451459+JustArt1m@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.DeviceNetwork.Components;
using Content.Server.Power.EntitySystems;
using Content.Server.PowerCell;
using Content.Shared.DeviceNetwork.Components;
using Content.Shared.Interaction;
using Content.Shared.PowerCell.Components;
using Content.Shared.Radio.EntitySystems;
using Content.Shared.Radio.Components;
using Content.Shared.DeviceNetwork.Systems;

namespace Content.Server.Radio.EntitySystems;

public sealed class JammerSystem : SharedJammerSystem
{
    [Dependency] private readonly PowerCellSystem _powerCell = default!;
    [Dependency] private readonly BatterySystem _battery = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedDeviceNetworkJammerSystem _jammer = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RadioJammerComponent, ActivateInWorldEvent>(OnActivate);
        SubscribeLocalEvent<ActiveRadioJammerComponent, PowerCellChangedEvent>(OnPowerCellChanged);
        SubscribeLocalEvent<RadioSendAttemptEvent>(OnRadioSendAttempt);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<ActiveRadioJammerComponent, RadioJammerComponent>();

        while (query.MoveNext(out var uid, out var _, out var jam))
        {

            if (_powerCell.TryGetBatteryFromSlot(uid, out var batteryUid, out var battery))
            {
                if (!_battery.TryUseCharge(batteryUid.Value, GetCurrentWattage((uid, jam)) * frameTime, battery))
                {
                    ChangeLEDState(uid, false);
                    RemComp<ActiveRadioJammerComponent>(uid);
                    RemComp<DeviceNetworkJammerComponent>(uid);
                }
                else
                {
                    var percentCharged = battery.CurrentCharge / battery.MaxCharge;
                    var chargeLevel = percentCharged switch
                    {
                        > 0.50f => RadioJammerChargeLevel.High,
                        < 0.15f => RadioJammerChargeLevel.Low,
                        _ => RadioJammerChargeLevel.Medium,
                    };
                    ChangeChargeLevel(uid, chargeLevel);
                }

            }

        }
    }

    private void OnActivate(Entity<RadioJammerComponent> ent, ref ActivateInWorldEvent args)
    {
        if (args.Handled || !args.Complex)
            return;

        var activated = !HasComp<ActiveRadioJammerComponent>(ent) &&
            _powerCell.TryGetBatteryFromSlot(ent.Owner, out var battery) &&
            battery.CurrentCharge > GetCurrentWattage(ent);
        if (activated)
        {
            ChangeLEDState(ent.Owner, true);
            EnsureComp<ActiveRadioJammerComponent>(ent);
            EnsureComp<DeviceNetworkJammerComponent>(ent, out var jammingComp);
            _jammer.SetRange((ent, jammingComp), GetCurrentRange(ent));
            _jammer.AddJammableNetwork((ent, jammingComp), DeviceNetworkComponent.DeviceNetIdDefaults.Wireless.ToString());
        }
        else
        {
            ChangeLEDState(ent.Owner, false);
            RemCompDeferred<ActiveRadioJammerComponent>(ent);
            RemCompDeferred<DeviceNetworkJammerComponent>(ent);
        }
        var state = Loc.GetString(activated ? "radio-jammer-component-on-state" : "radio-jammer-component-off-state");
        var message = Loc.GetString("radio-jammer-component-on-use", ("state", state));
        Popup.PopupEntity(message, args.User, args.User);
        args.Handled = true;
    }

    private void OnPowerCellChanged(Entity<ActiveRadioJammerComponent> ent, ref PowerCellChangedEvent args)
    {
        if (args.Ejected)
        {
            ChangeLEDState(ent.Owner, false);
            RemCompDeferred<ActiveRadioJammerComponent>(ent);
        }
    }

    private void OnRadioSendAttempt(ref RadioSendAttemptEvent args)
    {
        if (ShouldCancelSend(args.RadioSource))
        {
            args.Cancelled = true;
        }
    }

    private bool ShouldCancelSend(EntityUid sourceUid)
    {
        var source = Transform(sourceUid).Coordinates;
        var query = EntityQueryEnumerator<ActiveRadioJammerComponent, RadioJammerComponent, TransformComponent>();

        while (query.MoveNext(out var uid, out _, out var jam, out var transform))
        {
            if (_transform.InRange(source, transform.Coordinates, GetCurrentRange((uid, jam))))
            {
                return true;
            }
        }

        return false;
    }
}