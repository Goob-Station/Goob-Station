// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Cojoke <83733158+Cojoke-dot@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 chavonadelal <156101927+chavonadelal@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Mind;
using Content.Server.Roles;
using Content.Server.Roles.Jobs;
using Content.Shared.CharacterInfo;
using Content.Shared.Objectives;
using Content.Shared.Objectives.Components;
using Content.Shared.Objectives.Systems;

namespace Content.Server.CharacterInfo;

public sealed class CharacterInfoSystem : EntitySystem
{
    [Dependency] private readonly JobSystem _jobs = default!;
    [Dependency] private readonly MindSystem _minds = default!;
    [Dependency] private readonly RoleSystem _roles = default!;
    [Dependency] private readonly SharedObjectivesSystem _objectives = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<RequestCharacterInfoEvent>(OnRequestCharacterInfoEvent);
    }

    private void OnRequestCharacterInfoEvent(RequestCharacterInfoEvent msg, EntitySessionEventArgs args)
    {
        if (!args.SenderSession.AttachedEntity.HasValue
            || args.SenderSession.AttachedEntity != GetEntity(msg.NetEntity))
            return;

        var entity = args.SenderSession.AttachedEntity.Value;

        var objectives = new Dictionary<string, List<ObjectiveInfo>>();
        var jobTitle = Loc.GetString("character-info-no-profession");
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
        }

        RaiseNetworkEvent(new CharacterInfoEvent(GetNetEntity(entity), jobTitle, objectives, briefing), args.SenderSession);
    }
}
