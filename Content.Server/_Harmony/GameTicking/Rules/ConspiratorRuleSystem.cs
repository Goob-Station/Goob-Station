using Content.Server._Harmony.GameTicking.Rules.Components;
using Content.Server.Antag;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.Roles;
using Content.Shared._Harmony.Conspirators.Components;
using Content.Shared._Harmony.Roles.Components;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mind;
using Content.Shared.Random.Helpers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;
using System.Diagnostics.CodeAnalysis;

namespace Content.Server._Harmony.GameTicking.Rules;

public sealed class ConspiratorRuleSystem : GameRuleSystem<ConspiratorRuleComponent>
{
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ConspiratorRoleComponent, GetBriefingEvent>(OnGetBriefing);
        SubscribeLocalEvent<ConspiratorRuleComponent, AfterAntagEntitySelectedEvent>(OnAntagSelected);
    }

    protected override void AppendRoundEndText(EntityUid uid,
        ConspiratorRuleComponent component,
        GameRuleComponent gameRule,
        ref RoundEndTextAppendEvent args)
    {
        base.AppendRoundEndText(uid, component, gameRule, ref args);

        var sessionData = _antag.GetAntagIdentifiers(uid);
        args.AddLine(Loc.GetString("conspirator-count", ("count", sessionData.Count)));
        foreach (var (_, data, name) in sessionData)
        {
            args.AddLine(Loc.GetString("conspirator-name-user",
                ("name", name),
                ("username", data.UserName)));
        }

        if (!_proto.TryIndex(component.Objective, out var objectiveProto))
            return;

        args.AddLine(Loc.GetString("conspirator-objective", ("objective", objectiveProto.Name)));
    }

    private void OnGetBriefing(Entity<ConspiratorRoleComponent> ent, ref GetBriefingEvent args)
    {
        args.Append(Loc.GetString("conspirator-identities"));

        var conspirators = AllEntityQuery<ConspiratorComponent>();
        while (conspirators.MoveNext(out var id, out _))
        {
            args.Append(Loc.GetString("conspirator-name", ("name", Name(id))));
        }
    }

    private void OnAntagSelected(Entity<ConspiratorRuleComponent> ent, ref AfterAntagEntitySelectedEvent args)
    {
        if (!_mind.TryGetMind(args.Session, out var mindId, out var mind))
            return;

        if (ent.Comp.Objective is null)
        {
            if (GetRandomObjectivePrototype(ent.Comp, out var objectiveProtoId))
                ent.Comp.Objective = objectiveProtoId;
        }

        if (ent.Comp.Objective is not null)
            _mind.TryAddObjective(mindId, mind, ent.Comp.Objective);
    }

    private bool GetRandomObjectivePrototype(ConspiratorRuleComponent comp, [NotNullWhen(true)] out EntProtoId? objectiveProto)
    {
        objectiveProto = null;

        if (!_proto.TryIndex(comp.ObjectiveGroup, out var group))
            return false;

        var objectives = group.Weights.ShallowClone();
        while (_random.TryPickAndTake(objectives, out var proto))
        {
            objectiveProto = proto!;
            return true;
        }

        return false;
    }
}
