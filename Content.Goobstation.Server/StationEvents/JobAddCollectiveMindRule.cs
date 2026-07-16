// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Chat.Managers;
using Content.Server.Mind;
using Content.Server.StationEvents.Events;
using Content.Shared.Chat;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mind.Components;
using Content.Shared.Roles.Jobs;
using Content.Shared._Starlight.CollectiveMind;
using Robust.Shared.Player;
using Content.Shared.Mind;

namespace Content.Goobstation.Server.StationEvents;

public sealed class JobAddCollectiveMindRule : StationEventSystem<JobAddCollectiveMindRuleComponent>
{
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly SharedJobSystem _job = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;

    protected override void Started(EntityUid uid, JobAddCollectiveMindRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        var query = EntityQueryEnumerator<MindContainerComponent>();
        while (query.MoveNext(out var target, out var mindContainer))
        {
            if (mindContainer.Mind == null)
                continue;

            foreach (var proto in component.Affected)
            {
                if (_job.MindHasJobWithId(mindContainer.Mind, proto))
                {
                    EnsureComp<CollectiveMindComponent>(target).Channels.Add(component.Channel);
                    if (component.Message != null &&
                        TryComp<MindComponent>(mindContainer.Mind, out var mind) &&
                        _player.TryGetSessionById(mind.UserId, out var session))
                    {
                        var message = Loc.GetString("chat-manager-server-wrap-message", ("message", Loc.GetString(component.Message)));
                        _chat.ChatMessageToOne(ChatChannel.Local, message, message, EntityUid.Invalid, false, session.Channel);
                    }
                    break;
                }
            }
        }
    }
}
