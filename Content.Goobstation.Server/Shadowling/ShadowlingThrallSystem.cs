using Content.Server.Actions;
using Content.Server.Antag;
using Content.Server.Language;
using Content.Server.Mind;
using Content.Server.Roles;
using Content.Shared._EE.Shadowling;
using Content.Shared._EE.Shadowling.Components;
using Content.Shared._EE.Shadowling.Thrall;
using Content.Shared.Actions;
using Content.Shared.Examine;
using Content.Shared.Overlays.Switchable;

namespace Content.Server._EE.Shadowling;


/// <summary>
/// This handles Thralls antag briefing and abilities
/// </summary>
public sealed class ShadowlingThrallSystem : EntitySystem
{
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly LanguageSystem _language = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly RoleSystem _roles = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ThrallComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<ThrallComponent, ComponentRemove>(OnRemove);

        SubscribeLocalEvent<ThrallComponent, ExaminedEvent>(OnExamined);
    }

    private void OnStartup(EntityUid uid, ThrallComponent component, ComponentStartup args)
    {
        // antag stuff
        if (!_mind.TryGetMind(uid, out var mindId, out var mind))
            return;

        if (mindId == default || !_roles.MindHasRole<ShadowlingRoleComponent>(mindId))
        {
            _roles.MindAddRole(mindId, "MindRoleThrall");
        }

        if (mind?.Session != null)
            _antag.SendBriefing(uid, Loc.GetString("thrall-role-greeting"), Color.MediumPurple, component.ThrallConverted);

        _language.AddLanguage(uid, component.SlingLanguageId);


        var nightVision = EnsureComp<NightVisionComponent>(uid);
        nightVision.ToggleAction = "ActionThrallDarksight"; // todo: not sure if this is needed, need to test it without it

        // Remove the night vision action because thrall darksight does the same thing, so why have 2 actions
        if (nightVision.ToggleActionEntity is null)
            return;
        _actions.RemoveAction(nightVision.ToggleActionEntity.Value);

        // Add Thrall Abilities
        if (!TryComp<ActionsComponent>(uid, out var actions))
            return;

        EnsureComp<ThrallGuiseComponent>(uid);
        _actions.AddAction(
            uid,
            ref component.ActionThrallDarksightEntity,
            component.ActionThrallDarksight,
            component: actions);

        _actions.AddAction(
            uid,
            ref component.ActionGuiseEntity,
            component.ActionGuise,
            component: actions);
    }

    private void OnRemove(EntityUid uid, ThrallComponent component, ComponentRemove args)
    {
        if (_mind.TryGetMind(uid, out var mindId, out _))
            _roles.MindRemoveRole<ShadowlingRoleComponent>(mindId);

        _actions.RemoveAction(component.ActionThrallDarksightEntity);
        _actions.RemoveAction(component.ActionGuiseEntity);

        RemComp<NightVisionComponent>(uid);
        RemComp<ThrallGuiseComponent>(uid);

        if (HasComp<LesserShadowlingComponent>(uid))
            RemCompDeferred<LesserShadowlingComponent>(uid);

        if (component.Converter == null)
            return;

        // Adjust lightning resistance for shadowling
        var shadowling = component.Converter.Value;
        var shadowlingComp = EntityManager.GetComponent<ShadowlingComponent>(shadowling);
        var shadowlingSystem = EntityManager.System<ShadowlingSystem>();

        shadowlingSystem.OnThrallRemoved(shadowling, uid, shadowlingComp);
    }

    private void OnExamined(EntityUid uid, ThrallComponent component, ExaminedEvent args)
    {
        if (!HasComp<ShadowlingComponent>(args.Examiner))
            return;

        if (component.Converter == null)
            return;

        if (component.Converter == args.Examiner)
            args.PushMarkup($"[color=red]{Loc.GetString("shadowling-thrall-examined")}[/color]"); // Indicates that it is your Thrall
    }
}
