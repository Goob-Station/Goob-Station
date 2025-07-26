// SPDX-FileCopyrightText: 2025 ThunderBear2006 <bearthunder06@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Server.Silicon.MalfAI.Rules;
using Content.Goobstation.Shared.MisandryBox.Smites;
using Content.Goobstation.Shared.Silicon.MalfAI;
using Content.Goobstation.Shared.Silicon.MalfAI.Components;
using Content.Server.AlertLevel;
using Content.Server.Audio;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.RoundEnd;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared.Chat;
using Content.Shared.Database;
using Content.Shared.Mobs.Components;
using Robust.Server.Audio;
using Robust.Shared.Audio;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Silicon.MalfAI;

public sealed partial class MalfStationAISystem : SharedMalfStationAISystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly AlertLevelSystem _alert = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly ServerGlobalSoundSystem _sound = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly ThunderstrikeSystem _smite = default!;
    [Dependency] private readonly RoundEndSystem _roundEnd = default!;

    public void StartDoomsDayDevice(Entity<MalfStationAIComponent> entity)
    {
        _adminLogManager.Add(LogType.Action, LogImpact.Extreme, $"{ToPrettyString(entity)} has activated AI doomsday.");

        var gamerule = EntityQuery<MalfAIRuleComponent>().First();

        gamerule.TimeDoomDeviceStarted = _timing.CurTime;

        gamerule.DoomDeviceStarting = true;
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<MalfAIRuleComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            if (!comp.DoomDeviceStarting)
                continue;

            UpdateDoomsDayDevice((uid, comp));
        }
    }

    private void UpdateDoomsDayDevice(Entity<MalfAIRuleComponent> entity)
    {
        if (!IsAIAliveAndOnStation(entity.Comp.AIEntity, entity.Comp.Station))
        {
            OnDoomsDayCancelled(entity);
            return;
        }

        if (entity.Comp.DoomFluffMessagesIndex < entity.Comp.DoomFluffMessageLocs.Count)
        {
            if (_timing.CurTime - entity.Comp.LastDoomDeviceMessageTime < entity.Comp.TimeUntilNextDoomDeviceMessage)
                return;

            SendFluffMessage(entity.Comp.AIEntity, Loc.GetString(entity.Comp.DoomFluffMessageLocs[entity.Comp.DoomFluffMessagesIndex++]), entity.Comp.FluffMessageSoundSpecifier);

            entity.Comp.TimeUntilNextDoomDeviceMessage = TimeSpan.FromSeconds(_robustRandom.Next(1, 5));

            entity.Comp.LastDoomDeviceMessageTime = _timing.CurTime;

            return;
        }

        if (!entity.Comp.DoomDeviceActive)
        {
            entity.Comp.DoomDeviceActive = true;

            OnDoomsDayActive(entity);

            return;
        }

        if (_timing.CurTime - entity.Comp.LastDoomDeviceMessageTime > entity.Comp.TimeUntilAlarmStart && !entity.Comp.PlayedAlarm)
        {
            _sound.PlayGlobalOnStation(entity.Comp.Station, _audio.ResolveSound(entity.Comp.AlarmSpecifier), AudioParams.Default);
            entity.Comp.PlayedAlarm = true;
            return;
        }

        if (_timing.CurTime - entity.Comp.LastDoomDeviceMessageTime < entity.Comp.TimeUntilDoomSetOff)
            return;

        AnnihilateOrganics(entity);
    }

    private void OnDoomsDayActive(Entity<MalfAIRuleComponent> entity)
    {
        _chat.DispatchStationAnnouncement(
            entity.Comp.AIEntity,
            Loc.GetString(entity.Comp.DoomAnnouncementStartLoc),
            null,
            true,
            entity.Comp.DoomAnnouncementSoundSpecifier,
            Color.Red
        );

        _alert.SetLevel(entity.Comp.Station, "delta", false, true, true, true);
    }

    private void OnDoomsDayCancelled(Entity<MalfAIRuleComponent> entity)
    {
        entity.Comp.DoomDeviceStarting = false;
        entity.Comp.DoomDeviceActive = false;

        _alert.SetLevel(entity.Comp.Station, "green", true, true, true);
    }

    private void AnnihilateOrganics(Entity<MalfAIRuleComponent> entity)
    {
        var query = EntityQueryEnumerator<MobStateComponent>();

        while (query.MoveNext(out var uid, out var _))
        {
            if (_station.GetOwningStation(uid) is not { } station || station != entity.Comp.Station)
                continue;

            if (HasComp<SiliconComponent>(uid) || HasComp<MalfStationAIComponent>(uid))
                continue;

            var coords = Transform(uid).Coordinates;

            _smite.CreateLighting(coords);

            QueueDel(uid);

            Spawn("Ash", coords);
        }

        entity.Comp.DoomDeviceActive = false;
        entity.Comp.DoomDeviceStarting = false;

        _roundEnd.EndRound();
    }

    private void SendFluffMessage(EntityUid entity, string msg, SoundSpecifier fluffSound)
    {
        if (!TryComp<ActorComponent>(entity, out var actor))
            return;

        _chatManager.ChatMessageToOne(ChatChannel.Notifications, msg, msg, default, false, actor.PlayerSession.Channel);

        _audio.PlayGlobal(fluffSound, Filter.SinglePlayer(actor.PlayerSession), true, AudioParams.Default);
    }
}