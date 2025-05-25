// SPDX-FileCopyrightText: 2024 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Errant <35878406+Errant-4@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Fildrance <fildrance@gmail.com>
// SPDX-FileCopyrightText: 2024 IProduceWidgets <107586145+IProduceWidgets@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 JustCone <141039037+JustCone14@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Mervill <mervills.email@gmail.com>
// SPDX-FileCopyrightText: 2024 PJBot <pieterjan.briers+bot@gmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 PopGamer46 <yt1popgamer@gmail.com>
// SPDX-FileCopyrightText: 2024 ScarKy0 <scarky0@onet.eu>
// SPDX-FileCopyrightText: 2024 Spessmann <156740760+Spessmann@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Winkarst <74284083+Winkarst-cpu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 coolboy911 <85909253+coolboy911@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 lunarcomets <140772713+lunarcomets@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2024 saintmuntzer <47153094+saintmuntzer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Doors.Components;
using Robust.Shared.Serialization;
using Content.Shared.Electrocution;

namespace Content.Shared.Silicons.StationAi;

public abstract partial class SharedStationAiSystem
{
    // Handles airlock radial

    private void InitializeAirlock()
    {
        SubscribeLocalEvent<DoorBoltComponent, StationAiBoltEvent>(OnAirlockBolt);
        SubscribeLocalEvent<AirlockComponent, StationAiEmergencyAccessEvent>(OnAirlockEmergencyAccess);
        SubscribeLocalEvent<ElectrifiedComponent, StationAiElectrifiedEvent>(OnElectrified);
    }

    /// <summary>
    /// Attempts to bolt door. If wire was cut (AI or for bolts) or its not powered - notifies AI and does nothing.
    /// </summary>
    private void OnAirlockBolt(EntityUid ent, DoorBoltComponent component, StationAiBoltEvent args)
    {
        if (component.BoltWireCut)
        {
            ShowDeviceNotRespondingPopup(args.User);
            return;
        }

        var setResult = _doors.TrySetBoltDown((ent, component), args.Bolted, args.User, predicted: true);
        if (!setResult)
        {
            ShowDeviceNotRespondingPopup(args.User);
        }
    }

    /// <summary>
    /// Attempts to toggle the door's emergency access. If wire was cut (AI) or its not powered - notifies AI and does nothing.
    /// </summary>
    private void OnAirlockEmergencyAccess(EntityUid ent, AirlockComponent component, StationAiEmergencyAccessEvent args)
    {
        if (!PowerReceiver.IsPowered(ent))
        {
            ShowDeviceNotRespondingPopup(args.User);
            return;
        }

        _airlocks.SetEmergencyAccess((ent, component), args.EmergencyAccess, args.User, predicted: true);
    }

    /// <summary>
    /// Attempts to electrify the door. If wire was cut (AI or for one of power-wires) or its not powered - notifies AI and does nothing.
    /// </summary>
    private void OnElectrified(EntityUid ent, ElectrifiedComponent component, StationAiElectrifiedEvent args)
    {
        if (
            component.IsWireCut
            || !PowerReceiver.IsPowered(ent)
        )
        {
            ShowDeviceNotRespondingPopup(args.User);
            return;
        }

        _electrify.SetElectrified((ent, component), args.Electrified);
        var soundToPlay = component.Enabled
            ? component.AirlockElectrifyDisabled
            : component.AirlockElectrifyEnabled;
        _audio.PlayLocal(soundToPlay, ent, args.User);
    }
}

/// <summary> Event for StationAI attempt at bolting/unbolting door. </summary>
[Serializable, NetSerializable]
public sealed class StationAiBoltEvent : BaseStationAiAction
{
    /// <summary> Marker, should be door bolted or unbolted. </summary>
    public bool Bolted;
}

/// <summary> Event for StationAI attempt at setting emergency access for door on/off. </summary>
[Serializable, NetSerializable]
public sealed class StationAiEmergencyAccessEvent : BaseStationAiAction
{
    /// <summary> Marker, should door have emergency access on or off. </summary>
    public bool EmergencyAccess;
}

/// <summary> Event for StationAI attempt at electrifying/de-electrifying door. </summary>
[Serializable, NetSerializable]
public sealed class StationAiElectrifiedEvent : BaseStationAiAction
{
    /// <summary> Marker, should door be electrified or no. </summary>
    public bool Electrified;
}