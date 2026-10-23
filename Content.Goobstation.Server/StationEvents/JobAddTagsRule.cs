// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Chat.Managers;
using Content.Server.Mind;
using Content.Server.StationEvents.Events;
using Content.Shared.Chat;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mind.Components;
using Content.Shared.Roles.Jobs;
using Content.Shared.Tag;
using Robust.Server.Player;

namespace Content.Goobstation.Server.StationEvents;

public sealed partial class JobAddTagsRule : StationEventSystem<JobAddTagsRuleComponent>
{
    [Dependency] private IChatManager _chat = default!;
    [Dependency] private MindSystem _mind = default!;
    [Dependency] private SharedJobSystem _job = default!;
    [Dependency] private TagSystem _tag = default!;
    [Dependency] private IPlayerManager _player = default!;

    protected override void Started(EntityUid uid, JobAddTagsRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
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
                    _tag.AddTags(target, component.Tags);
                    if (component.Message != null && _player.TryGetSessionByEntity(mindContainer.Mind.Value, out var session))
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
