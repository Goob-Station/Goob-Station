using Content.Goobstation.Shared.Overlays;
using Content.Goobstation.Shared.Shadowling.Components;
using Content.Goobstation.Shared.Shadowling.Components.Abilities.Thrall;
using Content.Server.Actions;
using Content.Server.Antag;
using Content.Server.Mind;
using Content.Server.Roles;
using Content.Shared.Actions;
using Content.Shared.Examine;

namespace Content.Goobstation.Server.Shadowling.Systems;

/// <summary>
/// This handles Thralls antag briefing and abilities
/// </summary>
public sealed class ShadowlingThrallSystem : EntitySystem
{
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly RoleSystem _roles = default!;
    [Dependency] private readonly ShadowlingSystem _shadowling = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ThrallComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<ThrallComponent, ComponentShutdown>(OnRemove);
        SubscribeLocalEvent<ThrallComponent, ExaminedEvent>(OnExamined);
    }

    private void OnStartup(EntityUid uid, ThrallComponent component, ComponentStartup args)
    {
        // antag stuff
        if (!_mind.TryGetMind(uid, out var mindId, out _))
            return;

        if (!_roles.MindHasRole<ShadowlingRoleComponent>(mindId))
            _roles.MindAddRole(mindId, "MindRoleThrall");

        _antag.SendBriefing(uid, Loc.GetString("thrall-role-greeting"), Color.MediumPurple, component.ThrallConverted);

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

    private void OnRemove(EntityUid uid, ThrallComponent component, ComponentShutdown args)
    {
        if (_mind.TryGetMind(uid, out var mindId, out _))
            _roles.MindRemoveRole<ShadowlingRoleComponent>(mindId);

        _actions.RemoveAction(component.ActionThrallDarksightEntity);
        _actions.RemoveAction(component.ActionGuiseEntity);

        RemComp<NightVisionComponent>(uid);
        RemComp<ThrallGuiseComponent>(uid);
        RemComp<LesserShadowlingComponent>(uid);

        if (component.Converter == null)
            return;

        // Adjust lightning resistance for shadowling
        var shadowling = component.Converter.Value;
        if (TryComp<ShadowlingComponent>(shadowling, out var shadowlingComp))
            _shadowling.OnThrallRemoved(shadowling, uid, shadowlingComp);
    }

    private void OnExamined(EntityUid uid, ThrallComponent component, ExaminedEvent args)
    {
        if (!HasComp<ShadowlingComponent>(args.Examiner)
            || component.Converter != args.Examiner)
            return;

        args.PushMarkup($"[color=red]{Loc.GetString("shadowling-thrall-examined")}[/color]"); // Indicates that it is your Thrall
    }
}
