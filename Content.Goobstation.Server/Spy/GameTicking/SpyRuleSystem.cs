using System.Text;
using Content.Goobstation.Server.Spy.Roles;
using Content.Goobstation.Shared.Spy;
using Content.Server.Antag;
using Content.Server.GameTicking.Rules;
using Content.Server.PDA.Ringer;
using Content.Server.Roles;
using Content.Server.Traitor.Uplink;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mind;
using Content.Shared.NPC.Systems;
using Content.Shared.PDA;
using Content.Shared.Random.Helpers;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Spy.GameTicking;

/// <summary>
/// This handles...
/// </summary>
public sealed class SpyRuleSystem : GameRuleSystem<SpyRuleComponent>
{
    [Dependency] private readonly SharedMindSystem _mindSystem = default!;
    [Dependency] private readonly SharedSpyBountySystem _bountySystem = default!;
    [Dependency] private readonly SharedRoleSystem _roleSystem = default!;
    [Dependency] private readonly NpcFactionSystem _npcFaction = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly UplinkSystem _uplink = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;


    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SpyRuleComponent, AfterAntagEntitySelectedEvent>(AfterEntitySelected);
    }

    private void AfterEntitySelected(Entity<SpyRuleComponent> ent, ref AfterAntagEntitySelectedEvent args)
    {
        Log.Debug($"AfterAntagEntitySelected {ToPrettyString(ent)}");
        MakeSpy(args.EntityUid, ent);
    }

    public bool MakeSpy(EntityUid spy, SpyRuleComponent component)
    {
        //Grab the mind if it wasn't provided
        if (!_mindSystem.TryGetMind(spy, out var mindId, out var mind))
            return false;

        component.SpyMinds.Add(mindId);
        var issuer = _random.Pick(_prototypeManager.Index(component.ObjectiveIssuers));


        _roleSystem.MindHasRole<SpyRoleComponent>(mindId, out var spyRole);

        Note[]? code;
        var pdaNullable = _uplink.FindUplinkTarget(spy);
        if (pdaNullable is not { } pda || !_uplink.AddUplink(spy, 0))
            return false;

        // Give traitors their codewords and uplink code to keep in their character info menu
        code = EnsureComp<RingerUplinkComponent>(pda).Code;
        EnsureComp<SpyUplinkComponent>(pda);

        _antag.SendBriefing(spy, GenerateSpyBriefing(code, issuer), Color.Crimson, component.GreetSoundNotification);
        if (spyRole is not null)
        {
            AddComp<RoleBriefingComponent>(spyRole.Value.Owner);
            Comp<RoleBriefingComponent>(spyRole.Value.Owner).Briefing = GenerateSpyCharBriefing(code, issuer);
        }

        // Give traitors their codewords and uplink code to keep in their character info menu
        // Change the faction
        _npcFaction.RemoveFaction(spy, component.NanoTrasenFaction, false);
        _npcFaction.AddFaction(spy, component.SyndicateFaction);

        return true;
    }

    private string GenerateSpyBriefing(Note[]? code, string issuer)
    {
        var sb = new StringBuilder();
        sb.AppendLine("\n" + Loc.GetString($"traitor-{issuer}-intro"));
        if (code is not null)
        {
            sb.AppendLine("\n" + Loc.GetString($"traitor-{issuer}-uplink"));
            sb.AppendLine(Loc.GetString($"traitor-role-uplink-code-short",
                ("code", string.Join("-", code).Replace("sharp", "#"))));
        }
        sb.AppendLine("\n" + Loc.GetString($"spy-briefing-rob"));
        return sb.ToString();
    }

    private string GenerateSpyCharBriefing(Note[]? code, string issuer)
    {
        var sb = new StringBuilder();
        sb.AppendLine("\n" + Loc.GetString($"traitor-{issuer}-intro"));
        if (code is not null)
        {
            sb.AppendLine("\n" + Loc.GetString($"traitor-{issuer}-uplink"));
            sb.AppendLine(Loc.GetString($"traitor-role-uplink-code",
                ("code", string.Join("-", code).Replace("sharp", "#"))));
        }

        sb.AppendLine("\n" + Loc.GetString($"spy-briefing-rob"));
        return sb.ToString();
    }

    protected override void Started(EntityUid uid,
        SpyRuleComponent component,
        GameRuleComponent gameRule,
        GameRuleStartedEvent args)
    {
        _bountySystem.CreateDbEntity();
        _bountySystem.SetupBounties();
    }
}
