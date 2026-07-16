// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Traitor;
using Content.Server.Antag;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Mind;
using Content.Server.Objectives;
using Content.Server.PDA.Ringer;
using Content.Server.Roles;
using Content.Server.Traitor.Uplink;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Mind;
using Content.Shared.NPC.Systems;
using Content.Shared.PDA;
using Content.Shared.Random.Helpers;
using Content.Shared.Roles;
using Content.Shared.Roles.Components;
using Content.Shared.Roles.Jobs;
using Content.Shared.Roles.RoleCodeword;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using System.Linq;
using System.Text;
using Content.Server.Codewords;
using Content.Server.Database;
using Content.Shared.Clumsy;
using Content.Server.Popups;
using Content.Shared.CCVar;
using Content.Goobstation.Common.CCVar;
using Robust.Shared.Configuration;

namespace Content.Server.GameTicking.Rules;

// goobstation - heavily edited.
// do not touch unless you want to shoot yourself in the leg
public sealed class TraitorRuleSystem : GameRuleSystem<TraitorRuleComponent>
{
    private static readonly Color TraitorCodewordColor = Color.FromHex("#cc3b3b");

    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly SharedJobSystem _jobs = default!;
    [Dependency] private readonly MindSystem _mindSystem = default!;
    [Dependency] private readonly NpcFactionSystem _npcFaction = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedRoleCodewordSystem _roleCodewordSystem = default!;
    [Dependency] private readonly SharedRoleSystem _roleSystem = default!;
    [Dependency] private readonly UplinkSystem _uplink = default!;
    [Dependency] private readonly CodewordSystem _codewordSystem = default!;
    [Dependency] private readonly PopupSystem _popup = default!; // goob edit
    [Dependency] private readonly IConfigurationManager _cfg = default!; // goob edit
    [Dependency] private readonly GoobCommonUplinkSystem _goobUplink = default!;

    public override void Initialize()
    {
        base.Initialize();

        Log.Level = LogLevel.Debug;

        SubscribeLocalEvent<TraitorRuleComponent, AfterAntagEntitySelectedEvent>(AfterEntitySelected);
        SubscribeLocalEvent<TraitorRuleComponent, ObjectivesTextPrependEvent>(OnObjectivesTextPrepend);
    }

    private void AfterEntitySelected(Entity<TraitorRuleComponent> ent, ref AfterAntagEntitySelectedEvent args)
    {
        Log.Debug($"AfterAntagEntitySelected {ToPrettyString(ent)}");
        MakeTraitor(args.EntityUid, ent);
    }

    public bool MakeTraitor(EntityUid traitor, TraitorRuleComponent component)
    {
        Log.Debug($"MakeTraitor {ToPrettyString(traitor)} - start");
        var factionCodewords = _codewordSystem.GetCodewords(component.CodewordFactionPrototypeId);

        //Grab the mind if it wasn't provided
        if (!_mindSystem.TryGetMind(traitor, out var mindId, out var mind))
        {
            Log.Debug($"MakeTraitor {ToPrettyString(traitor)}  - failed, no Mind found");
            return false;
        }

        var briefing = "";

        if (component.GiveCodewords)
        {
            Log.Debug($"MakeTraitor {ToPrettyString(traitor)} - added codewords flufftext to briefing");
            briefing = Loc.GetString("traitor-role-codewords-short", ("codewords", string.Join(", ", factionCodewords)));
        }

        var issuer = _random.Pick(_prototypeManager.Index(component.ObjectiveIssuers));

        string? uplinkBriefing = null; // Goob
        string? uplinkBriefingShort = null; // Goob

        if (component.GiveUplink)
        {
            Log.Debug($"MakeTraitor {ToPrettyString(traitor)} - Uplink start");
            // Calculate the amount of currency on the uplink.
            var startingBalance = component.StartingBalance;
            if (_jobs.MindTryGetJob(mindId, out var prototype))
                // Goob Start
                startingBalance = Math.Max(startingBalance - prototype.AntagAdvantage, 0);

            var uplinkPreference = _goobUplink.GetUplinkPreference(mindId);

            if (!_uplink.TryAddUplink(traitor, startingBalance, uplinkPreference, out _, out var setupEvent))
                return false;

            if (setupEvent != null)
            {
                uplinkBriefing = setupEvent.Value.BriefingEntry;
                uplinkBriefingShort = setupEvent.Value.BriefingEntryShort;
            }
            else // Fallback ooplink
                // Goob End
            {
                uplinkBriefing = Loc.GetString("traitor-role-uplink-implant");
                uplinkBriefingShort = Loc.GetString("traitor-role-uplink-implant-short");
            }
            /* Goob
            Yeah idk apparently we arent using this check comment of me crashing out below.
            todo goobstation clean up this whole file in relation to upstream version
            pain

            // Choose and generate an Uplink, and return the uplink code if applicable
            Log.Debug($"MakeTraitor {ToPrettyString(traitor)} - Uplink request start");
            var uplinkParams = RequestUplink(traitor, startingBalance, briefing);
            code = uplinkParams.Item1;
            briefing = uplinkParams.Item2;
            Log.Debug($"MakeTraitor {ToPrettyString(traitor)} - Uplink request completed");
            */
        }

        string[]? codewords = null;
        if (component.GiveCodewords)
        {
            // Goob start
            Log.Debug($"MakeTraitor {ToPrettyString(traitor)} - added codewords flufftext to briefing");
            briefing = Loc.GetString("traitor-role-codewords-short", ("codewords", string.Join(", ", factionCodewords)));
            // Goob end
            Log.Debug($"MakeTraitor {ToPrettyString(traitor)} - set codewords from component");
            codewords = factionCodewords;
        }

        if (component.GiveBriefing)
        {
            _antag.SendBriefing(traitor, GenerateBriefing(codewords, uplinkBriefing, issuer), Color.Crimson, component.GreetSoundNotification); // Goob
            Log.Debug($"MakeTraitor {ToPrettyString(traitor)} - Sent the Briefing");
        }

        Log.Debug($"MakeTraitor {ToPrettyString(traitor)} - Adding TraitorMind");
        component.TraitorMinds.Add(mindId);

        // Assign briefing
        //Since this provides neither an antag/job prototype, nor antag status/roletype,
        //and is intrinsically related to the traitor role
        //it does not need to be a separate Mind Role Entity
        _roleSystem.MindHasRole<TraitorRoleComponent>(mindId, out var traitorRole);
        if (traitorRole is not null)
        {
            // goob edit - traitor flavor.
            traitorRole.Value.Comp2.ObjectiveIssuer = issuer;

            Log.Debug($"MakeTraitor {ToPrettyString(traitor)} - Add traitor briefing components");
            EnsureComp<RoleBriefingComponent>(traitorRole.Value.Owner, out var briefingComp);
            // Goobstation Change - If you remove this, we lose ringtones and flavor in char menu. Upstream's version sucks.
            briefingComp.Briefing = GenerateBriefingCharacter(codewords, uplinkBriefingShort, issuer);
        }

        var color = TraitorCodewordColor; // Fall back to a dark red Syndicate color if a prototype is not found

        // The mind entity is stored in nullspace with a PVS override for the owner, so only they can see the codewords.
        var codewordComp = EnsureComp<RoleCodewordComponent>(mindId);
        _roleCodewordSystem.SetRoleCodewords((mindId, codewordComp), "traitor", factionCodewords.ToList(), color);

        // Change the faction
        Log.Debug($"MakeTraitor {ToPrettyString(traitor)} - Change faction");
        _npcFaction.RemoveFaction(traitor, component.NanoTrasenFaction, false);
        _npcFaction.AddFaction(traitor, component.SyndicateFaction);

        // goob edit - clumsy antag no more
        var shouldRemoveClumsy = _cfg.GetCVar(GoobCVars.RemoveClumsyOnAntag);
        if (TryComp<ClumsyComponent>(traitor, out var clumsy) && shouldRemoveClumsy)
        {
            // if not for the clown car i would've nuked it off the planet
            clumsy.ClumsyCatching = false;
            clumsy.ClumsyDefib = false;
            clumsy.ClumsyGuns = false;
            clumsy.ClumsyVaulting = false;
            clumsy.ClumsyHypo = false;

            _popup.PopupEntity(Loc.GetString("antag-gain-remove-clumsy"), traitor, traitor, Shared.Popups.PopupType.Medium);
        }

        Log.Debug($"MakeTraitor {ToPrettyString(traitor)} - Finished");
        return true;
    }
    /*
     Goobstation john fucking station nuked this whole ass thing for whatever implement we have now
     Im not fucking fixing this. For all i care this file might as well be entirely goob.
     Im recommenting it in for upstream clarity.
     TODO GOOBSTATION. MOVE UPLINK SHIT TO GOOBMOD AND UNFUCK THIS???
     ffs
    private (Note[]?, string) RequestUplink(EntityUid traitor, FixedPoint2 startingBalance, string briefing)
    {
        var pda = _uplink.FindUplinkTarget(traitor);
        Note[]? code = null;

        Log.Debug($"MakeTraitor {ToPrettyString(traitor)} - Uplink add");
        var uplinked = _uplink.AddUplink(traitor, startingBalance, pda, true);

        if (pda is not null && uplinked)
        {
            Log.Debug($"MakeTraitor {ToPrettyString(traitor)} - Uplink is PDA");
            // Codes are only generated if the uplink is a PDA
            var ev = new GenerateUplinkCodeEvent();
            RaiseLocalEvent(pda.Value, ref ev);

            if (ev.Code is { } generatedCode)
            {
                code = generatedCode;

                // If giveUplink is false the uplink code part is omitted
                briefing = string.Format("{0}\n{1}",
                    briefing,
                    Loc.GetString("traitor-role-uplink-code-short", ("code", string.Join("-", code).Replace("sharp", "#"))));
                return (code, briefing);
            }

            Log.Error($"MakeTraitor {ToPrettyString(traitor)} failed to generate an uplink code on {ToPrettyString(pda)}.");
        }
        else if (pda is null && uplinked)
        {
            Log.Debug($"MakeTraitor {ToPrettyString(traitor)} - Uplink is implant");
            briefing += "\n" + Loc.GetString("traitor-role-uplink-implant-short");
        }
        else
        {
            Log.Error($"MakeTraitor failed on {ToPrettyString(traitor)} - No uplink could be added");
        }

        return (null, briefing);
    }
    Goob end
    */

    // TODO: AntagCodewordsComponent
    private void OnObjectivesTextPrepend(EntityUid uid, TraitorRuleComponent comp, ref ObjectivesTextPrependEvent args)
    {
        if (comp.GiveCodewords)
            args.Text += "\n" + Loc.GetString("traitor-round-end-codewords", ("codewords", string.Join(", ", _codewordSystem.GetCodewords(comp.CodewordFactionPrototypeId))));
    }

    // TODO: figure out how to handle this? add priority to briefing event?
    private string GenerateBriefing(string[]? codewords, string? uplinkBriefing, string objectiveIssuer)
    {
        var issuer = objectiveIssuer.Replace(" ", "").ToLower();
        var sb = new StringBuilder();
        sb.AppendLine(Loc.GetString($"traitor-{issuer}-intro"));

        if (uplinkBriefing != null)
        {
            sb.AppendLine("\n" + Loc.GetString($"traitor-{issuer}-uplink"));
            sb.AppendLine(uplinkBriefing);
        }
        else sb.AppendLine(Loc.GetString("traitor-role-nouplink"));

        if (codewords != null)
            sb.AppendLine("\n" + Loc.GetString("traitor-role-codewords", ("codewords", string.Join(", ", codewords))));

        sb.AppendLine("\n" + Loc.GetString("traitor-role-moreinfo"));

        return sb.ToString();
    }

    // Goobstation Change - Readd the character briefing text.
    private string GenerateBriefingCharacter(string[]? codewords, string? uplinkBriefingShort, string objectiveIssuer)
    {
        var issuer = objectiveIssuer.Replace(" ", "").ToLower();
        var sb = new StringBuilder();
        sb.AppendLine("\n" + Loc.GetString($"traitor-{issuer}-intro"));

        if (uplinkBriefingShort != null)
            sb.AppendLine(uplinkBriefingShort);
        else sb.AppendLine("\n" + Loc.GetString($"traitor-role-nouplink"));

        if (codewords != null)
            sb.AppendLine("\n" + Loc.GetString($"traitor-role-codewords-short", ("codewords", string.Join(", ", codewords))));

        sb.AppendLine("\n" + Loc.GetString($"traitor-role-allegiances"));
        sb.AppendLine(Loc.GetString($"traitor-{issuer}-allies"));

        sb.AppendLine("\n" + Loc.GetString($"traitor-role-notes"));
        sb.AppendLine(Loc.GetString($"traitor-{issuer}-goal"));

        return sb.ToString();
    }

    public List<(EntityUid Id, MindComponent Mind)> GetOtherTraitorMindsAliveAndConnected(MindComponent ourMind)
    {
        List<(EntityUid Id, MindComponent Mind)> allTraitors = new();

        var query = EntityQueryEnumerator<TraitorRuleComponent>();
        while (query.MoveNext(out var uid, out var traitor))
        {
            foreach (var role in GetOtherTraitorMindsAliveAndConnected(ourMind, (uid, traitor)))
            {
                if (!allTraitors.Contains(role))
                    allTraitors.Add(role);
            }
        }

        return allTraitors;
    }

    private List<(EntityUid Id, MindComponent Mind)> GetOtherTraitorMindsAliveAndConnected(MindComponent ourMind, Entity<TraitorRuleComponent> rule)
    {
        var traitors = new List<(EntityUid Id, MindComponent Mind)>();
        foreach (var mind in _antag.GetAntagMinds(rule.Owner))
        {
            if (mind.Comp == ourMind)
                continue;

            traitors.Add((mind, mind));
        }

        return traitors;
    }
}
