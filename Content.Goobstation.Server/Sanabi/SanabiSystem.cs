// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 LaCumbiaDelCoronavirus <90893484+LaCumbiaDelCoronavirus@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Content.Server.Chat.Systems;
using Content.Server.CriminalRecords.Systems;
using Content.Server.StationRecords.Systems;
using Content.Shared.StationRecords;
using Content.Server.Station.Systems;
using Content.Shared.CriminalRecords;
using Content.Shared.Security;
using Robust.Shared.Physics.Systems;
using Content.Goobstation.Server.MisandryBox;

namespace Content.Goobstation.Server.Sanabi.Systems;

public sealed class SanabiSystem : EntitySystem
{
    [Dependency] SharedTransformSystem _transformSystem = default!;
    [Dependency] SharedAudioSystem _audioSystem = default!;
    [Dependency] CriminalRecordsSystem _criminalRecords = default!;
    [Dependency] StationRecordsSystem _records = default!;
    [Dependency] StationSystem _stationSystem = default!;
    [Dependency] SharedJointSystem _jointSystem = default!;
    [Dependency] CatEmoteSpamCountermeasureSystem _C_RAM = default!;

    private static List<string> _sanabiPrefixes = new() { "ПРОКЛЯТИЕ 220", "ПРОКЛЯТИЕ САНАБИ", "ПРОКЛЯТИЕ SANABI", "САНАБИ", "CURSE OF 220", "CURSE OF SANABI", "SANABI" };

    private static EntProtoId _sanabiEntity = "SanabiImage";
    private readonly SoundSpecifier _sanabiSound = new SoundPathSpecifier("/Audio/Voice/Human/malescream_2.ogg")
    {
        Params = AudioParams.Default.WithVolume(-5f),
    };

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EntitySpokeEvent>(OnSpeak);
    }

    private void OnSpeak(EntitySpokeEvent chatArgs)
    {
        if (!_sanabiPrefixes.Exists(prefix => chatArgs.Message.IndexOf(prefix, StringComparison.OrdinalIgnoreCase) >= 0))
            return;

        var sender = chatArgs.Source;
        if (!_transformSystem.TryGetMapOrGridCoordinates(sender, out var lCoords) || lCoords is not { } sanabiCoords)
            return;

        var sanabiEntity = SpawnAttachedTo(_sanabiEntity, sanabiCoords);
        _audioSystem.PlayEntity(_audioSystem.ResolveSound(_sanabiSound), Filter.Broadcast(), sender, recordReplay: true);

        _jointSystem.CreateDistanceJoint(sanabiEntity, sender, minimumDistance: 0);
        _C_RAM.Add(sender);

        if (_stationSystem.GetStationInMap(_transformSystem.GetMapId(sanabiCoords)) is not { } station)
            return;

        if (!TryComp(sender, out MetaDataComponent? senderMetadataComp))
            return;

        if (_records.GetRecordByName(station, senderMetadataComp.EntityName) is not { } id)
            return;

        var senderRecord = new StationRecordKey(id, station);
        if (!_records.TryGetRecord<CriminalRecord>(senderRecord, out var criminalRecord))
            return;


        if (criminalRecord.Status != SecurityStatus.None && criminalRecord.Status != SecurityStatus.Detained && criminalRecord.Status != SecurityStatus.Perma)
            return;

        _criminalRecords.TryChangeStatus(senderRecord, SecurityStatus.Dangerous, "65% chance of 220");
    }
}
