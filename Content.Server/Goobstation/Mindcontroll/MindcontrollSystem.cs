using Content.Shared.Mindshield.Components;
using Content.Server.Administration.Logs;
using Content.Server.Mind;
using Content.Server.Popups;
using Content.Shared.Popups;
using Content.Server.Roles;
using Content.Shared.Database;
using Content.Server.GameTicking.Rules;
using Content.Server.Traitor.Systems;
using Content.Server.Antag;
using Content.Shared.Mind.Components;
using Content.Shared.Mind;
using Content.Server.Stunnable;
using Content.Shared.StatusIcon.Components;
using Content.Shared.StatusIcon;
using Robust.Shared.Prototypes;
using Content.Server.Revolutionary;
using Content.Server.Revolutionary.Components;
using Content.Shared.Revolutionary.Components;
using Content.Shared.Mindcontroll;
using Robust.Shared.GameObjects;

namespace Content.Server.Mindcontroll;

public sealed class MindcontrollSystem : SharedMindcontrollSystem
{
    [Dependency] private readonly IAdminLogManager _adminLogManager = default!;
    [Dependency] private readonly RoleSystem _roleSystem = default!;
    [Dependency] private readonly MindSystem _mindSystem = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MindcontrollComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<MindcontrollComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<MindcontrollComponent, MindAddedMessage>(OnMindAdded);
        SubscribeLocalEvent<MindcontrollComponent, MindRemovedMessage>(OnMindRemoved);
        SubscribeLocalEvent<MindcontrollRoleComponent, GetBriefingEvent>(OnGetBriefing);
    }
    public void OnStartup(EntityUid uid, MindcontrollComponent component, ComponentStartup arg)
    {
        EnsureComp<MindcontrollComponent>(uid);
        _stun.TryParalyze(uid, TimeSpan.FromSeconds(5f), true); //dont need this but, but its a still a good indicator from how Revulution and subverted silicone does it
    }
    public void OnShutdown(EntityUid uid, MindcontrollComponent component, ComponentShutdown arg)
    {
        _stun.TryParalyze(uid, TimeSpan.FromSeconds(5f), true);
        _mindSystem.TryGetMind(uid, out var mindId, out _);
        _roleSystem.MindTryRemoveRole<MindcontrollRoleComponent>(mindId);
        _adminLogManager.Add(LogType.Mind, LogImpact.Medium, $"{ToPrettyString(uid)} is no longer Mindcontrolled.");
        //if (_mindSystem.TryGetSession(mindId, out var session))
    }
    public void Start(EntityUid uid, MindcontrollComponent component)
    {
        if (component.Master == null)
            return;
        if (HasComp<MindShieldComponent>(uid))  //you somhow managed to implant somone whit a mindshield.
            return;
        if (uid == component.Master.Value)  //good jobb, you implanted yourself
            return;

        _mindSystem.TryGetMind(uid, out var mindId, out var mind);

        if (!_roleSystem.MindHasRole<MindcontrollRoleComponent>(mindId))
            _roleSystem.MindAddRole(mindId, new MindcontrollRoleComponent { PrototypeId = "Mindcontrolled", MasterUid = component.Master.Value });

        if (mind?.Session != null)
        {
            _popup.PopupEntity(Loc.GetString("You are Mindcontrolled"), uid, PopupType.LargeCaution);
            _antag.SendBriefing(mind.Session, Loc.GetString("You are Mindcontrolled \n OBEY " + MetaData(component.Master.Value).EntityName), Color.Red, component.MindcontrollStartSound);
        }
        _adminLogManager.Add(LogType.Mind, LogImpact.Medium, $"{ToPrettyString(uid)} is Mindcontrolled by {ToPrettyString(component.Master.Value)}.");
    }
    private void OnMindAdded(EntityUid uid, MindcontrollComponent component, MindAddedMessage args)  //  OnMindAdded is if somone whit out a mind gets implanted, like Ian before given cognezine or somone dead ghost.
    {
        if (!_roleSystem.MindHasRole<MindcontrollRoleComponent>(args.Mind.Owner))
            Start(uid, component); //goes agein if comp added before mind.
    }
    private void OnMindRemoved(EntityUid uid, MindcontrollComponent component, MindRemovedMessage args)
    {
        _roleSystem.MindTryRemoveRole<MindcontrollRoleComponent>(args.Mind.Owner);
    }
    private void OnGetBriefing(Entity<MindcontrollRoleComponent> target, ref GetBriefingEvent args)
    {
        if (!TryComp<MindComponent>(target.Owner, out var mind) || mind.OwnedEntity == null)
            return;

        args.Append(MakeBriefing(target));
    }
    private string MakeBriefing(Entity<MindcontrollRoleComponent> target)
    {
        var briefing = Loc.GetString("YOU ARE MINDCONTROLLED");
        if (target.Comp.MasterUid != null) // Returns null if Master is gibbed
        {
            TryComp<MetaDataComponent>(target.Comp.MasterUid, out var metadata);
            if (metadata != null)
                briefing += "\n " + Loc.GetString("Obey: ") + metadata.EntityName + "\n";
        }
        return briefing;
    }
}
