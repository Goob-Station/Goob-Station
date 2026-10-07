// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Server.Objectives;
using Content.Goobstation.Shared.Administration.Objectives;
using Content.Server._Goobstation.Wizard.Components;
using Content.Server.Administration.Logs;
using Content.Server.Administration.Managers;
using Content.Server.EUI;
using Content.Server.Objectives;
using Content.Server.Objectives.Components;
using Content.Server.Objectives.Systems;
using Content.Shared.Administration;
using Content.Shared.Database;
using Content.Shared.Mind;
using Content.Shared.Mobs.Systems;
using Content.Shared.Objectives.Components;
using Content.Shared.Prototypes;
using Content.Shared.Roles.Jobs;
using Robust.Shared.Map.Components;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.Administration.Objectives;

/// <summary>
/// Logic behind the admin objectives panel: listing, adding (prototype, custom, steal) and removing objectives.
/// Kept separate from the EUI so other systems can reuse it.
/// </summary>
public sealed class AdminObjectivesSystem : EntitySystem
{
    [Dependency] private readonly EuiManager _euis = default!;
    [Dependency] private readonly IAdminLogManager _adminLog = default!;
    [Dependency] private readonly IAdminManager _adminMan = default!;
    [Dependency] private readonly IComponentFactory _compFactory = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly AdminStealConditionSystem _steal = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly ObjectivesSystem _objectives = default!;
    [Dependency] private readonly SharedJobSystem _job = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly TargetObjectiveSystem _target = default!;

    public const string CustomObjectiveProto = "AdminCustomObjective";
    public const string StealObjectiveProto = "AdminStealObjective";
    public const string DefaultIssuer = "objective-issuer-unknown";

    private List<AdminObjectiveIssuerEntry> _issuers = new();

    public override void Initialize()
    {
        base.Initialize();

        _proto.PrototypesReloaded += OnPrototypesReloaded;
        CacheIssuers();
    }

    public override void Shutdown()
    {
        base.Shutdown();

        _proto.PrototypesReloaded -= OnPrototypesReloaded;
    }

    private void OnPrototypesReloaded(PrototypesReloadedEventArgs args)
    {
        if (args.WasModified<EntityPrototype>())
            CacheIssuers();
    }

    /// <summary>
    /// Collects every distinct issuer used by an objective prototype so the admin can pick one for custom objectives.
    /// </summary>
    private void CacheIssuers()
    {
        var issuers = new HashSet<string> { DefaultIssuer };
        foreach (var proto in _proto.EnumeratePrototypes<EntityPrototype>())
        {
            if (proto.Abstract || !proto.TryComp<ObjectiveComponent>(out var comp, _compFactory))
                continue;

            string issuer = comp.Issuer;
            if (!string.IsNullOrWhiteSpace(issuer) && Loc.TryGetString(issuer, out _))
                issuers.Add(issuer);
        }

        _issuers = issuers
            .Select(i => new AdminObjectiveIssuerEntry(i, FormattedMessage.RemoveMarkupPermissive(Loc.GetString(i))))
            .OrderBy(i => i.Name)
            .ToList();
    }

    /// <summary>
    /// Opens the objectives panel for a mind on an admin's screen.
    /// </summary>
    public void OpenPanel(ICommonSession admin, EntityUid mindId)
    {
        if (!_adminMan.HasAdminFlag(admin, AdminFlags.Admin))
            return;

        var name = TryComp<MindComponent>(mindId, out var mind)
            ? mind.CharacterName ?? ToPrettyString(mindId).ToString()
            : ToPrettyString(mindId).ToString();

        _euis.OpenEui(new AdminObjectivesEui(mindId, name), admin);
    }

    public AdminObjectivesEuiState BuildState(EntityUid mindId, string playerName)
    {
        if (!TryComp<MindComponent>(mindId, out var mind))
            return new AdminObjectivesEuiState(playerName, false, new(), new(), new(), new());

        return new AdminObjectivesEuiState(
            playerName,
            true,
            GetObjectives(mindId, mind),
            GetPrototypes(),
            GetTargets(mindId),
            _issuers);
    }

    private List<AdminObjectiveEntry> GetObjectives(EntityUid mindId, MindComponent mind)
    {
        var result = new List<AdminObjectiveEntry>(mind.Objectives.Count);
        foreach (var objective in mind.Objectives)
        {
            if (!TryComp<ObjectiveComponent>(objective, out var comp))
                continue;

            // GetTarget logs an error when the objective has no TargetObjectiveComponent, so only ask targeted ones
            string? targetName = null;
            if (TryComp<TargetObjectiveComponent>(objective, out var targetComp) &&
                _target.GetTarget(objective, out var target, targetComp) &&
                TryComp<MindComponent>(target, out var targetMind))
            {
                targetName = targetMind.CharacterName;
            }

            result.Add(new AdminObjectiveEntry(
                GetNetEntity(objective),
                MetaData(objective).EntityPrototype?.ID,
                _objectives.GetInfo(objective, mindId, mind),
                FormattedMessage.RemoveMarkupPermissive(comp.LocIssuer),
                targetName));
        }

        return result;
    }

    private List<AdminObjectivePrototypeEntry> GetPrototypes()
    {
        var result = new List<AdminObjectivePrototypeEntry>();
        foreach (var id in _objectives.Objectives())
        {
            // these have their own tabs
            if (id is CustomObjectiveProto or StealObjectiveProto)
                continue;

            if (!_proto.TryIndex<EntityPrototype>(id, out var proto))
                continue;

            result.Add(new AdminObjectivePrototypeEntry(
                id,
                proto.Name,
                proto.HasComponent<TargetObjectiveComponent>(_compFactory)));
        }

        return result;
    }

    private List<AdminObjectiveTargetEntry> GetTargets(EntityUid exclude)
    {
        var result = new List<AdminObjectiveTargetEntry>();
        var query = EntityQueryEnumerator<MindComponent>();
        while (query.MoveNext(out var uid, out var mind))
        {
            if (uid == exclude || mind.OriginalOwnerUserId == null)
                continue;

            var dead = mind.OwnedEntity is not { } body || _mobState.IsDead(body);
            result.Add(new AdminObjectiveTargetEntry(
                GetNetEntity(uid),
                mind.CharacterName ?? Loc.GetString("admin-objectives-unknown-name"),
                _job.MindTryGetJobName(uid),
                dead));
        }

        return result.OrderBy(t => t.CharacterName).ToList();
    }

    /// <summary>
    /// Adds an objective from a prototype, optionally designating its target.
    /// </summary>
    /// <param name="targetMind">Mind to use as the target of kill / teach a lesson / etc. objectives. Random if null.</param>
    /// <param name="bypass">Skip the objective's requirements (role, uniqueness...).</param>
    public bool TryAddPrototype(
        ICommonSession admin,
        EntityUid mindId,
        string protoId,
        EntityUid? targetMind,
        bool bypass,
        out string message)
    {
        message = string.Empty;

        if (!TryComp<MindComponent>(mindId, out var mind))
        {
            message = Loc.GetString("admin-objectives-error-no-mind");
            return false;
        }

        if (!_proto.TryIndex<EntityPrototype>(protoId, out var proto) ||
            !proto.HasComponent<ObjectiveComponent>(_compFactory))
        {
            message = Loc.GetString("admin-objectives-error-bad-prototype", ("proto", protoId));
            return false;
        }

        if (targetMind is { } t && !HasComp<MindComponent>(t))
        {
            message = Loc.GetString("admin-objectives-error-bad-target");
            return false;
        }

        var objective = _objectives.TryCreateObjective(mindId, mind, protoId, bypass, uid =>
        {
            if (targetMind is not { } target || !TryComp<TargetObjectiveComponent>(uid, out var targetComp))
                return;

            // keep the title up to date if the target changes body or name
            EnsureComp<DynamicObjectiveTargetMindComponent>(target);
            _target.SetTarget(uid, target, targetComp);
        });

        if (objective == null)
        {
            message = Loc.GetString(bypass
                ? "admin-objectives-error-assign-failed-bypass"
                : "admin-objectives-error-assign-failed");
            return false;
        }

        FinishAdd(admin, mindId, mind, objective.Value, protoId);
        message = Loc.GetString("admin-objectives-added", ("objective", MetaData(objective.Value).EntityName));
        return true;
    }

    /// <summary>
    /// Adds a custom objective that is always complete and shows whatever the admin typed.
    /// </summary>
    public bool TryAddCustom(
        ICommonSession admin,
        EntityUid mindId,
        string title,
        string description,
        string issuer,
        out string message)
    {
        message = string.Empty;

        if (!TryComp<MindComponent>(mindId, out var mind))
        {
            message = Loc.GetString("admin-objectives-error-no-mind");
            return false;
        }

        title = title.Trim();
        if (title.Length == 0)
        {
            message = Loc.GetString("admin-objectives-error-empty-title");
            return false;
        }

        if (title.Length > AdminObjectivesLimits.MaxTitleLength)
            title = title[..AdminObjectivesLimits.MaxTitleLength];

        description = description.Trim();
        if (description.Length > AdminObjectivesLimits.MaxDescriptionLength)
            description = description[..AdminObjectivesLimits.MaxDescriptionLength];

        var objective = _objectives.TryCreateObjective(mindId, mind, CustomObjectiveProto, true, null);
        if (objective is not { } uid)
        {
            message = Loc.GetString("admin-objectives-error-assign-failed-bypass");
            return false;
        }

        // the title is shown in the round end summary which parses markup, so don't let it through
        _meta.SetEntityName(uid, FormattedMessage.EscapeText(title));
        _meta.SetEntityDescription(uid, description);
        _objectives.SetIssuer(uid, ValidateIssuer(issuer));

        FinishAdd(admin, mindId, mind, uid, CustomObjectiveProto, title);
        message = Loc.GetString("admin-objectives-added", ("objective", title));
        return true;
    }

    /// <summary>
    /// Adds an objective to steal one specific entity. Title, description and icon are filled in from it.
    /// </summary>
    public bool TryAddSteal(
        ICommonSession admin,
        EntityUid mindId,
        EntityUid item,
        string issuer,
        out string message)
    {
        message = string.Empty;

        if (!TryComp<MindComponent>(mindId, out var mind))
        {
            message = Loc.GetString("admin-objectives-error-no-mind");
            return false;
        }

        if (!Exists(item) || TerminatingOrDeleted(item))
        {
            message = Loc.GetString("admin-objectives-error-bad-item");
            return false;
        }

        // stealing yourself would be instantly complete
        if (item == mindId || item == mind.OwnedEntity || HasComp<MapComponent>(item) || HasComp<MapGridComponent>(item))
        {
            message = Loc.GetString("admin-objectives-error-bad-item");
            return false;
        }

        var objective = _objectives.TryCreateObjective(mindId, mind, StealObjectiveProto, true,
            obj => _steal.SetTarget(obj, item));
        if (objective is not { } uid)
        {
            message = Loc.GetString("admin-objectives-error-assign-failed-bypass");
            return false;
        }

        _objectives.SetIssuer(uid, ValidateIssuer(issuer));

        FinishAdd(admin, mindId, mind, uid, StealObjectiveProto, ToPrettyString(item).ToString());
        message = Loc.GetString("admin-objectives-added", ("objective", MetaData(uid).EntityName));
        return true;
    }

    public bool TryRemove(ICommonSession admin, EntityUid mindId, EntityUid objective, out string message)
    {
        message = string.Empty;

        if (!TryComp<MindComponent>(mindId, out var mind))
        {
            message = Loc.GetString("admin-objectives-error-no-mind");
            return false;
        }

        var index = mind.Objectives.IndexOf(objective);
        if (index < 0)
        {
            message = Loc.GetString("admin-objectives-error-not-found");
            return false;
        }

        var name = MetaData(objective).EntityName;
        if (!_mind.TryRemoveObjective(mindId, mind, index))
        {
            message = Loc.GetString("admin-objectives-error-not-found");
            return false;
        }

        // SharedMindSystem only dirties when clearing, and the list is networked
        Dirty(mindId, mind);

        _adminLog.Add(LogType.Mind, LogImpact.Medium,
            $"Admin {admin.Name} removed objective \"{name}\" from {ToPrettyString(mindId)}");
        message = Loc.GetString("admin-objectives-removed", ("objective", name));
        return true;
    }

    private void FinishAdd(
        ICommonSession admin,
        EntityUid mindId,
        MindComponent mind,
        EntityUid objective,
        string protoId,
        string? extra = null)
    {
        _mind.AddObjective(mindId, mind, objective);
        Dirty(mindId, mind);

        _adminLog.Add(LogType.Mind, LogImpact.Medium,
            $"Admin {admin.Name} added objective {protoId} \"{MetaData(objective).EntityName}\" {extra} to {ToPrettyString(mindId)}");
    }

    private string ValidateIssuer(string issuer)
    {
        return _issuers.Any(i => i.Id == issuer) ? issuer : DefaultIssuer;
    }
}
