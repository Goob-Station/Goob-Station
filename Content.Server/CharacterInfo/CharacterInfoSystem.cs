// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Administration.Logs;
using Content.Server.Mind;
using Content.Server.Preferences.Managers;
using Content.Server.Roles;
using Content.Server.Roles.Jobs;
using Content.Shared.CCVar;
using Content.Shared.CharacterInfo;
using Content.Shared.Database;
using Content.Shared.DetailExaminable;
using Content.Shared.Objectives;
using Content.Shared.Objectives.Components;
using Content.Shared.Objectives.Systems;
using Content.Shared.Preferences;
using Robust.Shared.Configuration;
using Robust.Shared.Utility;

namespace Content.Server.CharacterInfo;

public sealed class CharacterInfoSystem : EntitySystem
{
    [Dependency] private readonly IAdminLogManager _adminLog = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly JobSystem _jobs = default!;
    [Dependency] private readonly MindSystem _minds = default!;
    [Dependency] private readonly IServerPreferencesManager _preferences = default!;
    [Dependency] private readonly RoleSystem _roles = default!;
    [Dependency] private readonly SharedObjectivesSystem _objectives = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<RequestCharacterInfoEvent>(OnRequestCharacterInfoEvent);
        SubscribeNetworkEvent<UpdateDetailExaminableEvent>(OnUpdateDetailExaminableEvent);
    }

    private void OnRequestCharacterInfoEvent(RequestCharacterInfoEvent msg, EntitySessionEventArgs args)
    {
        if (!args.SenderSession.AttachedEntity.HasValue
            || args.SenderSession.AttachedEntity != GetEntity(msg.NetEntity))
            return;

        var entity = args.SenderSession.AttachedEntity.Value;

        var objectives = new Dictionary<string, List<ObjectiveInfo>>();
        var jobTitle = Loc.GetString("character-info-no-profession");
        var memories = new Dictionary<string, string>(); //Pirate banking
        string? briefing = null;
        if (_minds.TryGetMind(entity, out var mindId, out var mind))
        {
            // Get objectives
            foreach (var objective in mind.Objectives)
            {
                var info = _objectives.GetInfo(objective, mindId, mind);
                if (info == null)
                    continue;

                // group objectives by their issuer
                var issuer = Comp<ObjectiveComponent>(objective).LocIssuer;
                if (!objectives.ContainsKey(issuer))
                    objectives[issuer] = new List<ObjectiveInfo>();
                objectives[issuer].Add(info.Value);
            }

            if (_jobs.MindTryGetJobName(mindId, out var jobName))
                jobTitle = jobName;

            // Get briefing
            briefing = _roles.MindGetBriefing(mindId);

            //Pirate banking
            // Get memories
            foreach (var memory in mind.AllMemories)
            {
                memories[memory.Name] = memory.Value;
            }
            //Pirate banking end
        }

        var detailExaminable = TryComp<DetailExaminableComponent>(entity, out var detail)
            ? detail.Content
            : Loc.GetString("flavor-text-placeholder");

        RaiseNetworkEvent(new CharacterInfoEvent(GetNetEntity(entity), jobTitle, objectives, briefing, detailExaminable, memories), args.SenderSession); //Pirate banking
    }

    // Pirate: allow editing the round description in-game.
    private void OnUpdateDetailExaminableEvent(UpdateDetailExaminableEvent msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { } entity)
            return;

        var newContent = FormattedMessage.RemoveMarkupOrThrow(msg.Content);
        var maxFlavorTextLength = _cfg.GetCVar(CCVars.MaxFlavorTextLength);
        if (newContent.Length > maxFlavorTextLength)
            newContent = newContent[..maxFlavorTextLength];

        var detail = EnsureComp<DetailExaminableComponent>(entity);
        detail.Content = newContent;

        var preferences = _preferences.GetPreferences(args.SenderSession.UserId);
        if (preferences.SelectedCharacter is HumanoidCharacterProfile profile)
            _ = _preferences.SetProfile(args.SenderSession.UserId, preferences.SelectedCharacterIndex, profile.WithFlavorText(newContent));

        _adminLog.Add(LogType.Identity, LogImpact.Medium, $"{ToPrettyString(entity):user} updated their round description");

        Dirty(entity, detail);
    }
}
