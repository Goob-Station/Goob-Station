using Content.Goobstation.Shared.Cyberpsychosis;
using Content.Shared._Shitmed.Body.Organ;
using Content.Shared._Shitmed.BodyEffects;
using Content.Shared.Body.Events;
using Content.Shared.Body.Organ;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Damage;
using Content.Shared.Emp;
using Content.Shared.Movement.Components;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Cyberware;

/// <summary>
/// Handles everything to do with cybernetics.
/// </summary>
public sealed class CyberneticsSystem : EntitySystem
{
    [Dependency] private readonly BodyPartEffectSystem _partEffect = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private EntityQuery<CyberneticsComponent> _cyberwareQuery;

    public override void Initialize()
    {
        base.Initialize();

        _cyberwareQuery = GetEntityQuery<CyberneticsComponent>();

        SubscribeLocalEvent<CyberneticsComponent, EmpPulseEvent>(OnEmpPulse);

        SubscribeLocalEvent<CyberneticsComponent, OrganAddedToBodyEvent>(OnOrganAdded);
        SubscribeLocalEvent<CyberneticsComponent, OrganRemovedFromBodyEvent>(OnOrganRemoved);

        SubscribeLocalEvent<CyberneticsComponent, BodyPartAddedEvent>(OnPartChanged);
        SubscribeLocalEvent<CyberneticsComponent, BodyPartRemovedEvent>(OnPartChanged);

        SubscribeLocalEvent<CyberneticsComponent, ComponentInit>(OnInit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_net.IsServer)
            return;

        var query = EntityQueryEnumerator<CyberneticsComponent>();
        while (query.MoveNext(out var uid, out var ware))
        {
            if (ware.RebootUntil is not { } until || _timing.CurTime < until)
                continue;

            ware.RebootUntil = null;
            Dirty(uid, ware);
            SetEnabled((uid, ware), true);
        }
    }

    private void OnOrganAdded(Entity<CyberneticsComponent> ent, ref OrganAddedToBodyEvent args)
        => RefreshCyberware(args.Body);

    private void OnOrganRemoved(Entity<CyberneticsComponent> ent, ref OrganRemovedFromBodyEvent args)
        => RefreshCyberware(args.OldBody);

    private void OnPartChanged<T>(Entity<CyberneticsComponent> ent, ref T args)
    {
        if (TryComp<BodyPartComponent>(ent, out var part) && part.Body is { } body)
            RefreshCyberware(body);
    }

    private void OnInit(Entity<CyberneticsComponent> ent, ref ComponentInit args)
    {
        if (TryGetBody(ent, out var body))
            RefreshCyberware(body);
    }

    private void RefreshCyberware(EntityUid body)
    {
        if (!_net.IsServer || TerminatingOrDeleted(body))
            return;

        if (TryGetImplants(body, out _))
            EnsureComp<CyberSanityComponent>(body);
    }

    public bool TryGetImplants(EntityUid body, out List<Entity<CyberneticsComponent>> implants)
    {
        implants = [];

        foreach (var part in _body.GetBodyChildren(body))
        {
            if (_cyberwareQuery.TryComp(part.Id, out var partWare))
                implants.Add((part.Id, partWare));

            foreach (var organ in _body.GetPartOrgans(part.Id, part.Component))
                if (_cyberwareQuery.TryComp(organ.Id, out var organWare))
                    implants.Add((organ.Id, organWare));
        }

        return implants.Count > 0;
    }

    /// <summary>
    /// Used when you have multiple implants that can have varying clocks that also depend on each-other.
    /// Think of things like smartlinks implants not getting an overclock buff if 1 of the 2 is not overclocked.
    /// </summary>
    public int LowestClock<T>(EntityUid body) where T : IComponent
    {
        if (!TryGetImplants(body, out var implants))
            return 0;

        var step = 0;
        var found = false;

        foreach (var implant in implants)
        {
            if (!HasComp<T>(implant))
                continue;

            step = found ? Math.Min(step, implant.Comp.ClockStep) : implant.Comp.ClockStep;
            found = true;
        }

        return step;
    }

    public bool TryGetBody(EntityUid implant, out EntityUid body)
    {
        body = EntityUid.Invalid;

        if (TryComp<OrganComponent>(implant, out var organ))
            body = organ.Body ?? EntityUid.Invalid;
        else if (TryComp<BodyPartComponent>(implant, out var part))
            body = part.Body ?? EntityUid.Invalid;

        return body.IsValid();
    }

    public bool IsEnabled(EntityUid implant)
        => !_cyberwareQuery.TryComp(implant, out var ware) || ware.Enabled;

    public void SetEnabled(Entity<CyberneticsComponent> ent, bool enabled)
    {
        if (ent.Comp.Enabled == enabled)
            return;

        ent.Comp.Enabled = enabled;
        Dirty(ent);

        if (TryComp<OrganComponent>(ent, out var organ))
        {
            if (organ.Body is not { } body)
                return;

            var changed = new CyberwareChangedEvent(body);
            RaiseLocalEvent(ent, ref changed);

            var organEv = new OrganEnableChangedEvent(enabled);
            RaiseLocalEvent(ent, ref organEv);
        }
        else if (TryComp<BodyPartComponent>(ent, out var part))
        {
            if (part.Body is not { } body)
                return;

            var changed = new CyberwareChangedEvent(body);
            RaiseLocalEvent(ent, ref changed);

            SuspendMovement(ent, body, enabled);
            _partEffect.SetEffectsEnabled((ent, part), enabled);
        }
    }

    private void SuspendMovement(Entity<CyberneticsComponent> ent, EntityUid body, bool enabled)
    {
        if (!TryComp<MovementBodyPartComponent>(ent, out var movement))
            return;

        if (enabled)
        {
            if (!TryComp<SuspendedMovementComponent>(ent, out var authored))
                return;

            movement.WalkSpeed = authored.WalkSpeed;
            movement.SprintSpeed = authored.SprintSpeed;
            movement.Acceleration = authored.Acceleration;
            RemCompDeferred<SuspendedMovementComponent>(ent);
        }
        else
        {
            if (HasComp<SuspendedMovementComponent>(ent))
                return;

            var authored = AddComp<SuspendedMovementComponent>(ent);
            authored.WalkSpeed = movement.WalkSpeed;
            authored.SprintSpeed = movement.SprintSpeed;
            authored.Acceleration = movement.Acceleration;

            movement.WalkSpeed = MovementSpeedModifierComponent.DefaultBaseWalkSpeed;
            movement.SprintSpeed = MovementSpeedModifierComponent.DefaultBaseSprintSpeed;
            movement.Acceleration = MovementSpeedModifierComponent.DefaultAcceleration;
        }

        Dirty(ent.Owner, movement);

        _body.UpdateMovementSpeed(body);
    }

    public void SetClock(Entity<CyberneticsComponent> ent, int step)
    {
        step = Math.Clamp(step, ent.Comp.MinClock, ent.Comp.MaxClock);

        if (step == ent.Comp.ClockStep)
            return;

        ent.Comp.ClockStep = step;
        Dirty(ent);

        if (!TryGetBody(ent, out var body))
            return;

        var changed = new CyberwareChangedEvent(body);
        RaiseLocalEvent(ent, ref changed);
    }

    public void SetActive(Entity<CyberneticsComponent?> ent, bool active)
    {
        if (!Resolve(ent, ref ent.Comp, false) || ent.Comp.Active == active)
            return;

        ent.Comp.Active = active;
        Dirty(ent, ent.Comp);
    }

    public void SetOverloaded(Entity<CyberneticsComponent?> ent, bool overloaded)
    {
        if (!Resolve(ent, ref ent.Comp, false) || ent.Comp.Overloaded == overloaded)
            return;

        ent.Comp.Overloaded = overloaded;
        Dirty(ent, ent.Comp);
    }

    /// <summary>
    /// Turns off the implant while "rebooting".
    /// Used by emp's and such.
    /// </summary>
    public void Reboot(Entity<CyberneticsComponent> ent, TimeSpan duration)
    {
        ent.Comp.RebootUntil = _timing.CurTime + duration;
        Dirty(ent);
        SetEnabled(ent, false);
    }

    private void OnEmpPulse(Entity<CyberneticsComponent> ent, ref EmpPulseEvent args)
    {
        if (!ent.Comp.Enabled)
            return;

        args.Affected = true;

        Reboot(ent, args.Duration);

        if (TryComp<BodyPartComponent>(ent, out var part) && part.Body is { } body)
            ShockOwner(ent, part, body);
    }

    private void ShockOwner(Entity<CyberneticsComponent> ent, BodyPartComponent part, EntityUid body)
    {
        _damageable.TryChangeDamage(body,
            ent.Comp.EmpDamage,
            ignoreResistances: true,
            targetPart: _body.GetTargetBodyPart(part));
    }
}
