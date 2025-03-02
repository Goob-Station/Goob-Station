using Content.Server._Reserve.Revolutionary.UI;
using Content.Server.EUI;
using Content.Server.GameTicking.Rules;
using Content.Server.Mind;
using Content.Server.Popups;
using Content.Server.Revolutionary;
using Content.Server.Revolutionary.Components;
using Content.Shared.Humanoid;
using Content.Shared.IdentityManagement;
using Content.Shared.Mind.Components;
using Content.Shared.Mindshield.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Revolutionary.Components;
using Content.Shared.Roles;
using Content.Shared.Verbs;
using Content.Shared.Zombies;
using Robust.Shared.Physics;
using Robust.Shared.Timing;

namespace Content.Server._Reserve.Revolutionary;

public sealed class ConsentRevolutionarySystem : EntitySystem
{
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly EuiManager _euiMan = default!;
    [Dependency] private readonly RevolutionaryRuleSystem _revRule = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private float _accumulator = 0;
    private const float AccumulatorTreshold = 1f;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HeadRevolutionaryComponent, GetVerbsEvent<InnateVerb>>(OnInnateVerb);
        SubscribeLocalEvent<ConsentRevolutionaryComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<ConsentRevolutionaryComponent, MindRemovedMessage>(OnMindRemoved);
        SubscribeLocalEvent<ConsentRevolutionaryComponent, RoleAddedEvent>(OnRoleAdded);
    }

    private void OnInnateVerb(EntityUid uid, HeadRevolutionaryComponent comp, GetVerbsEvent<InnateVerb> args)
    {
        // Check if it's something convertable.
        if (!comp.OnlyConsentConvert
            || !comp.ConvertAbilityEnabled
            || !args.CanAccess
            || !args.CanInteract
            || HasComp<RevolutionaryComponent>(args.Target)
            || !_mobState.IsAlive(args.Target)
            || HasComp<ZombieComponent>(args.Target)
            )
            return;

        if (IsInConversionProcess(args.Target) || IsInConversionProcess(args.User))
            return;

        var alwaysConvertible = HasComp<AlwaysRevolutionaryConvertibleComponent>(args.Target);

        if ((!HasComp<HumanoidAppearanceComponent>(args.Target) ||
             !_mind.TryGetMind(args.Target, out var mindId, out var mind)
            ) && !alwaysConvertible)
            return;

        // Verb to convert people
        var verb = new InnateVerb()
        {
            Act = () =>
            {
                // Check that entity is still convertable
                if (!_revRule.IsConvertable(args.Target))
                    return;

                // Don't let convert people that denied request recently
                if (TryComp<ConsentRevolutionaryComponent>(args.Target, out var actConsRev) &&
                    actConsRev.LastRequestedTimeSpan != null &&
                    _timing.CurTime - actConsRev.LastRequestedTimeSpan < _timing.CurTime)
                {
                    _popup.PopupEntity(
                        Loc.GetString("rev-consent-convert-failed-convert-block", ("target", Identity.Entity(args.Target, EntityManager))),
                        args.Target,
                        args.User);
                    return;
                }

                // We don't hide verb if person has mindshield/command protection because that reveals it.
                // If verb was used in this situation, conversion will not proceed and both players will be alerted about.
                if (HasComp<MindShieldComponent>(args.Target) ||
                    HasComp<CommandStaffComponent>(args.Target))
                {
                    _popup.PopupEntity(
                        Loc.GetString("rev-consent-convert-attempted-to-be-converted", ("user", Identity.Entity(args.User, EntityManager))),
                        args.User,
                        args.Target,
                        PopupType.MediumCaution
                        );
                    _popup.PopupEntity(
                        Loc.GetString("rev-consent-convert-failed", ("target", Identity.Entity(args.Target, EntityManager))),
                        args.Target,
                        args.User,
                        PopupType.MediumCaution);
                    return;
                }

                RequestConsentConversionToEntity(args.Target, args.User);
            },
            Text = Loc.GetString("rev-verb-consent-convert-text"),
            Message = Loc.GetString("rev-verb-consent-convert-message")
        };

        args.Verbs.Add(verb);
    }

    public override void Update(float frameTime)
    {
        _accumulator += frameTime;

        if (_accumulator >= AccumulatorTreshold)
            _accumulator -= AccumulatorTreshold;
        else
            return;

        var query = EntityQueryEnumerator<ConsentRevolutionaryComponent>();
        while (query.MoveNext(out var uid, out var consentRev))
        {
            if (consentRev.IsConverter || consentRev.OtherMember == null)
            {
                continue;
            }

            // Convertor component
            if (!TryComp<ConsentRevolutionaryComponent>(consentRev.OtherMember, out var convertorConsentRev))
            {
                consentRev.OtherMember = null;
                continue;
            }

            // Check time
            if (consentRev.LastRequestedTimeSpan != null &&
                _timing.CurTime - consentRev.LastRequestedTimeSpan > consentRev.ResponseTime)
            {
                CancelRequest((uid, consentRev),
                    (consentRev.OtherMember.Value, convertorConsentRev),
                    reason: Loc.GetString("rev-consent-convert-failed-mid-convert-timeout"));
                continue;
            }

            // Check positions
            if (!_transform.InRange(Transform(uid).Coordinates,
                    Transform(consentRev.OtherMember.Value).Coordinates,
                    consentRev.MaxDistance)
                )
            {
                CancelRequest((uid, consentRev),
                    (consentRev.OtherMember.Value, convertorConsentRev),
                    reason: Loc.GetString("rev-consent-convert-failed-mid-convert-out-of-range"));
                continue;
            }
        }
    }

    private void OnMobStateChanged(EntityUid uid, ConsentRevolutionaryComponent consentRev, MobStateChangedEvent args)
    {
        if (consentRev.OtherMember == null || !TryComp<ConsentRevolutionaryComponent>(consentRev.OtherMember, out var otherConsentRev))
            return;

        if (args.NewMobState == MobState.Alive)
            return;

        if (consentRev.IsConverter)
        {
            CancelRequest((consentRev.OtherMember.Value, otherConsentRev),
                (uid, consentRev),
                reason: Loc.GetString("rev-consent-convert-failed-mid-convert-not-alive"));
        }
        else
        {
            CancelRequest((uid, consentRev),
                (consentRev.OtherMember.Value, otherConsentRev),
                reason: Loc.GetString("rev-consent-convert-failed-mid-convert-not-alive"));
        }
    }

    private void OnMindRemoved(Entity<ConsentRevolutionaryComponent> ent, ref MindRemovedMessage args)
    {
        if (ent.Comp.OtherMember == null || !TryComp<ConsentRevolutionaryComponent>(ent.Comp.OtherMember, out var otherConsentRev))
            return;

        if (ent.Comp.IsConverter)
        {
            CancelRequest((ent.Comp.OtherMember.Value, otherConsentRev),
                ent,
                reason: Loc.GetString("rev-consent-convert-failed-mid-convert-not-alive"));
        }
        else
        {
            CancelRequest(ent,
                (ent.Comp.OtherMember.Value, otherConsentRev),
                reason: Loc.GetString("rev-consent-convert-failed-mid-convert-not-alive"));
        }
    }

    private void OnRoleAdded(Entity<ConsentRevolutionaryComponent> ent, ref RoleAddedEvent args)
    {
        if (ent.Comp.OtherMember == null ||
            ent.Comp.IsConverter ||
            !TryComp<ConsentRevolutionaryComponent>(ent.Comp.OtherMember, out var otherConsentRev) ||
            !HasComp<RevolutionaryComponent>(ent))
            return;

        CancelRequest((ent.Comp.OtherMember.Value, otherConsentRev), ent);
    }

    /// <summary>
    /// Request conversion to entity
    /// </summary>
    /// <param name="target">Entity to request convert</param>
    /// <param name="converter">Entity that requests convert</param>
    public void RequestConsentConversionToEntity(EntityUid target, EntityUid converter)
    {
        // Start conversion
        if (_mind.TryGetMind(target, out var consentMindId, out var _) &&
            _mind.TryGetSession(consentMindId, out var session))
        {
            // Tell the converter that request was sent
            _popup.PopupEntity(
                Loc.GetString("rev-consent-convert-requested", ("target", Identity.Entity(target, EntityManager))),
                converter,
                converter);

            var window = new ConsentRequestedEui(target, converter, _revRule, this, _popup, EntityManager);

            // For target
            var targetComp = EnsureComp<ConsentRevolutionaryComponent>(target);
            targetComp.OtherMember = converter;
            targetComp.Window = window;
            targetComp.LastRequestedTimeSpan = _timing.CurTime;
            targetComp.IsConverter = false;

            // For converter
            var converterComp = EnsureComp<ConsentRevolutionaryComponent>(converter);
            converterComp.OtherMember = target;
            converterComp.IsConverter = true;

            _euiMan.OpenEui(window, session);
        }
        else
        {
            // Entity doesn't have mind (not controlled by player) to give response, but it's still convertable without it. We'll consent for them
            _popup.PopupEntity(
                Loc.GetString("rev-consent-convert-auto-accepted", ("target", Identity.Entity(target, EntityManager))),
                converter,
                converter);
            _revRule.ConvertEntityToRevolution(target, converter);
        }
    }

    /// <summary>
    /// Is entity is currently member of convert request.
    /// </summary>
    /// <param name="entity">Entity to check</param>
    /// <returns></returns>
    public bool IsInConversionProcess(EntityUid entity)
    {
        return TryComp<ConsentRevolutionaryComponent>(entity, out var consentRev)
               && consentRev.OtherMember != null;
    }

    /// <summary>
    /// Cancels convert request.
    /// </summary>
    /// <param name="target">Entity that being requested to convert</param>
    /// <param name="converter">Entity that requests convert</param>
    /// <param name="doBlock">If request block should be applied after cancel</param>
    /// <param name="reason">Reason for cancel that will be sent to players as popup. If null, nothing will be sent</param>
    public void CancelRequest(Entity<ConsentRevolutionaryComponent> target, Entity<ConsentRevolutionaryComponent> converter, bool doBlock = false, string? reason = null)
    {
        if (reason != null)
        {
            _popup.PopupEntity(
                reason,
                target,
                target,
                PopupType.MediumCaution
            );
            _popup.PopupEntity(
                reason,
                converter,
                converter,
                PopupType.MediumCaution);
        }

        CancelRequest(target.Comp, converter.Comp, doBlock);
    }

    /// <summary>
    /// Cancels convert request.
    /// </summary>
    /// <param name="target">Component of entity that being requested to convert</param>
    /// <param name="converter">Component of entity that requests convert</param>
    /// <param name="doBlock">If request block should be applied after cancel</param>
    public void CancelRequest(ConsentRevolutionaryComponent target, ConsentRevolutionaryComponent converter, bool doBlock = false)
    {
        target.OtherMember = null;

        if (target.Window != null)
        {
            target.Window.Close();
            target.Window = null;
        }

        target.LastRequestedTimeSpan = doBlock ? _timing.CurTime : null;

        converter.OtherMember = null;
    }
}
