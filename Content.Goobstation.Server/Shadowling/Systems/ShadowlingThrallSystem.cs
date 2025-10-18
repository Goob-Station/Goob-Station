using Content.Goobstation.Shared.Devil;
using Content.Goobstation.Shared.Overlays;
using Content.Goobstation.Shared.Shadowling.Components;
using Content.Goobstation.Shared.Shadowling.Components.Abilities.Thrall;
using Content.Server.Antag;
using Content.Server.Mind;
using Content.Server.Roles;
using Content.Shared._Starlight.CollectiveMind;
using Content.Shared.Examine;
using Content.Shared.IdentityManagement;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Shadowling.Systems;

/// <summary>
/// This handles Thralls antag briefing and abilities
/// </summary>
public sealed class ShadowlingThrallSystem : EntitySystem
{
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
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

    public ProtoId<CollectiveMindPrototype> ShadowMind = "Shadowmind";
    private void OnStartup(EntityUid uid, ThrallComponent component, ComponentStartup args)
    {
        // antag stuff
        if (!_mind.TryGetMind(uid, out var mindId, out _))
            return;

        if (!_roles.MindHasRole<ShadowlingRoleComponent>(mindId))
            _roles.MindAddRole(mindId, "MindRoleThrall");

        EnsureComp<CollectiveMindComponent>(uid).Channels.Add(ShadowMind);

        _antag.SendBriefing(uid, Loc.GetString("thrall-role-greeting"), Color.MediumPurple, component.ThrallConverted);
    }

    private void OnRemove(EntityUid uid, ThrallComponent component, ComponentShutdown args)
    {
        if (_mind.TryGetMind(uid, out var mindId, out _))
            _roles.MindRemoveRole<ShadowlingRoleComponent>(mindId);

        RemComp<NightVisionComponent>(uid);
        RemComp<ThrallGuiseComponent>(uid);
        RemComp<LesserShadowlingComponent>(uid);

        if (TryComp<CollectiveMindComponent>(uid, out var collective))
            collective.Channels.Remove(ShadowMind);

        if (component.Converter == null)
            return;

        // Adjust lightning resistance for shadowling
        var shadowling = component.Converter.Value;
        if (TryComp<ShadowlingComponent>(shadowling, out var shadowlingComp))
            _shadowling.OnThrallRemoved((shadowling, shadowlingComp));
    }

    private void OnExamined(EntityUid uid, ThrallComponent component, ExaminedEvent args)
    {
        if (HasComp<ShadowlingComponent>(args.Examiner)
            && component.Converter == args.Examiner)
            args.PushMarkup($"[color=red]{Loc.GetString("shadowling-thrall-examined")}[/color]"); // Indicates that it is your Thrall

        var ev = new IsEyesCoveredCheckEvent();
        RaiseLocalEvent(uid, ev);

        if (ev.IsEyesProtected)
            return;

        args.PushMarkup($"[color=pink]{Loc.GetString("shadowling-thrall-other-examined", ("target", Identity.Entity(uid, EntityManager)))}[/color]");
    }
}
