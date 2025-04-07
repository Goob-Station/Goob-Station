using Content.Goobstation.Shared.Mindcontrol;
using Content.Server.Administration.Logs;
using Content.Server.Antag;
using Content.Server.Mind;
using Content.Server.Popups;
using Content.Server.Roles;
using Content.Server.Stunnable;
using Content.Shared.Database;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Mindshield.Components;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Mindcontrol;

public sealed class MindcontrolSystem : EntitySystem
{
    [Dependency] private readonly IAdminLogManager _adminLogManager = default!;
    [Dependency] private readonly RoleSystem _roleSystem = default!;
    [Dependency] private readonly MindSystem _mindSystem = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    [ValidatePrototypeId<EntityPrototype>] static EntProtoId mindRole = "MindRoleBrainwashed";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MindcontrolledComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<MindcontrolledComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<MindcontrolledComponent, MindAddedMessage>(OnMindAdded);
        SubscribeLocalEvent<MindcontrolledComponent, MindRemovedMessage>(OnMindRemoved);
        SubscribeLocalEvent<MindcontrolledRoleComponent, GetBriefingEvent>(OnGetBriefing);
    }
    public void OnStartup(EntityUid uid, MindcontrolledComponent component, ComponentStartup arg)
    {
        _stun.TryParalyze(uid, TimeSpan.FromSeconds(5f), true); //dont need this but, but its a still a good indicator from how Revulution and subverted silicone does it
    }
    public void OnShutdown(EntityUid uid, MindcontrolledComponent component, ComponentShutdown arg)
    {
        _stun.TryParalyze(uid, TimeSpan.FromSeconds(5f), true);
        if (_mindSystem.TryGetMind(uid, out var mindId, out _))
            _roleSystem.MindTryRemoveRole<MindcontrolledRoleComponent>(mindId);
        _popup.PopupEntity(Loc.GetString("mindcontrol-popup-stop"), uid, PopupType.Large);
        _adminLogManager.Add(LogType.Mind, LogImpact.Medium, $"{ToPrettyString(uid)} is no longer Mindcontrolled.");
    }
    public void Start(EntityUid uid, MindcontrolledComponent component)
    {
        if (component.Master == null)
            return;
        if (HasComp<MindShieldComponent>(uid))  //you somhow managed to implant somone whit a mindshield.
            return;
        if (uid == component.Master.Value)  //good jobb, you implanted yourself
            return;
        if (!_mindSystem.TryGetMind(uid, out var mindId, out var mind))   //no mind, how can you mindcontrol whit no mind?
            return;

        _roleSystem.MindAddRole(mindId, mindRole.Id, silent: true);

        if (_roleSystem.MindHasRole<MindcontrolledRoleComponent>(mindId, out var mr))
            AddComp(mr.Value, new RoleBriefingComponent { Briefing = MakeBriefing(component.Master.Value) }, true);

        if (mind?.Session != null && !component.BriefingSent)
        {
            _popup.PopupEntity(Loc.GetString("mindcontrol-popup-start"), uid, PopupType.LargeCaution);
            _antag.SendBriefing(mind.Session, Loc.GetString("mindcontrol-briefing-start", ("master", (MetaData(component.Master.Value).EntityName))), Color.Red, component.MindcontrolStartSound);
            component.BriefingSent = true;
        }
        _adminLogManager.Add(LogType.Mind, LogImpact.Medium, $"{ToPrettyString(uid)} is Mindcontrolled by {ToPrettyString(component.Master.Value)}.");
    }
    private void OnMindAdded(EntityUid uid, MindcontrolledComponent component, MindAddedMessage args)  //  OnMindAdded is if somone whit out a mind gets implanted, like Ian before given cognezine or somone dead ghost.
    {
        if (!_roleSystem.MindHasRole<MindcontrolledRoleComponent>(args.Mind.Owner))
            Start(uid, component); //goes agein if comp added before mind.
    }
    private void OnMindRemoved(EntityUid uid, MindcontrolledComponent component, MindRemovedMessage args)
    {
        _roleSystem.MindTryRemoveRole<MindcontrolledRoleComponent>(args.Mind.Owner);
    }
    private void OnGetBriefing(Entity<MindcontrolledRoleComponent> target, ref GetBriefingEvent args)
    {
        if (!TryComp<MindComponent>(target.Owner, out var mind) || mind.OwnedEntity == null)
            return;

        args.Append(MakeBriefing(target.Comp.MasterUid));
    }
    private string MakeBriefing(EntityUid? masterId)
    {
        var briefing = Loc.GetString("mindcontrol-briefing-get");
        if (masterId != null) // Returns null if Master is gibbed
        {
            TryComp<MetaDataComponent>(masterId, out var metadata);
            if (metadata != null)
                briefing += "\n " + Loc.GetString("mindcontrol-briefing-get-master", ("master", metadata.EntityName)) + "\n";
        }
        return briefing;
    }
}
