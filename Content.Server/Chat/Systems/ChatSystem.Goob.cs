using System.Linq;
using Content.Goobstation.Common.VoxAudio;
using Content.Shared._Starlight.CollectiveMind;
using Content.Shared.Chat;
using Content.Shared.Database;
using Content.Shared.Station.Components;
using Robust.Shared.Audio;
using Robust.Shared.Player;

namespace Content.Server.Chat.Systems;

public sealed partial class ChatSystem
{
    // Goobstation - Starlight collective mind port
    private void SendCollectiveMindChat(EntityUid source, string message, CollectiveMindPrototype? collectiveMind)
    {
        if (_mobStateSystem.IsDead(source) || collectiveMind == null || message == "" || !TryComp<CollectiveMindComponent>(source, out var sourseCollectiveMindComp) || !sourseCollectiveMindComp.Minds.ContainsKey(collectiveMind.ID))
            return;

        var clients = Filter.Empty();
        var clientsSeeNames = Filter.Empty();
        var mindQuery = EntityQueryEnumerator<CollectiveMindComponent, ActorComponent>();
        while (mindQuery.MoveNext(out var uid, out var collectMindComp, out var actorComp))
        {
            if (_mobStateSystem.IsDead(uid))
                continue;

            if (collectMindComp.Minds.ContainsKey(collectiveMind.ID) || collectMindComp.HearAll)
            {
                if (collectMindComp.SeeAllNames)
                    clientsSeeNames.AddPlayer(actorComp.PlayerSession);
                else
                    clients.AddPlayer(actorComp.PlayerSession);
            }
        }

        var Number = $"{sourseCollectiveMindComp.Minds[collectiveMind.ID]}";

        var admins = _adminManager.ActiveAdmins
            .Select(p => p.Channel);

        string messageWrap = Loc.GetString("collective-mind-chat-wrap-message",
            ("message", message),
            ("channel", collectiveMind.LocalizedName),
            ("number", Number));
        string namedMessageWrap = Loc.GetString("collective-mind-chat-wrap-message-named",
            ("source", source),
            ("message", message),
            ("channel", collectiveMind.LocalizedName));
        string adminMessageWrap = Loc.GetString("collective-mind-chat-wrap-message-admin",
            ("source", source),
            ("message", message),
            ("channel", collectiveMind.LocalizedName),
            ("number", Number));

        _adminLogger.Add(LogType.Chat, LogImpact.Low, $"CollectiveMind chat from {ToPrettyString(source):Player}: {message}");

        // send to normal clients
        _chatManager.ChatMessageToManyFiltered(clients,
            ChatChannel.CollectiveMind,
            message,
            collectiveMind.ShowNames ? namedMessageWrap : messageWrap,
            source,
            false,
            true,
            collectiveMind.Color);

        // send to normal clients that should always see names, aka ghosts
        _chatManager.ChatMessageToManyFiltered(clientsSeeNames,
            ChatChannel.CollectiveMind,
            message,
            namedMessageWrap,
            source,
            false,
            true,
            collectiveMind.Color);

        // FOR ADMINS
        _chatManager.ChatMessageToMany(ChatChannel.CollectiveMind,
            message,
            adminMessageWrap,
            source,
            false,
            true,
            admins,
            collectiveMind.Color);
    }
}