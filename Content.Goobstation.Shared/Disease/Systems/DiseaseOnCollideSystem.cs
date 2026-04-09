using Content.Goobstation.Shared.Disease.Components;
using Content.Shared.Interaction;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Whitelist;
using Robust.Shared.Physics.Events;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Disease.Systems;

/// <summary>
/// This is used for spreading diseases on collide
/// </summary>
public sealed class DiseaseOnCollideSystem : EntitySystem
{
    [Dependency] private readonly SharedDiseaseSystem _disease = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DiseaseOnCollideComponent, DiseaseRelayedEvent<StartCollideEvent>>(OnCollide); //When stepping to close to others
        SubscribeLocalEvent<DiseaseOnCollideComponent, DiseaseRelayedEvent<PullStartedMessage>>(OnPull); //when pulling or being pulled
        SubscribeLocalEvent<DiseaseOnCollideComponent, DiseaseRelayedEvent<InteractHandEvent>>(OnHand); // when being given hug
        SubscribeLocalEvent<DiseaseOnCollideComponent, DiseaseRelayedEvent<BeforeInteractHandEvent>>(OnHand); //when giving hug

    }

    private void OnCollide(Entity<DiseaseOnCollideComponent> ent, ref DiseaseRelayedEvent<StartCollideEvent> arg)
    {
        if (_timing.CurTime < ent.Comp.Cooldown)
            return;
        ent.Comp.Cooldown = ent.Comp.CooldownInterval + _timing.CurTime;

        var host = arg.Args.OurEntity;
        var target = arg.Args.OtherEntity;
        var diseaseUid = Transform(ent.Owner).ParentUid;

        if (!TryComp<DiseaseComponent>(diseaseUid, out var diseaseComponent))
            return;

        OnContact(ent, host, target, (diseaseUid, diseaseComponent));
    }

    private void OnPull(Entity<DiseaseOnCollideComponent> ent, ref DiseaseRelayedEvent<PullStartedMessage> arg)
    {
        var diseaseUid = Transform(ent.Owner).ParentUid;
        var host = Transform(diseaseUid).ParentUid;
        var target = arg.Args.PulledUid;

        if (target == host)
            target = arg.Args.PullerUid;

        if (!TryComp<DiseaseComponent>(diseaseUid, out var diseaseComponent))
            return;

        OnContact(ent, host, target, (diseaseUid, diseaseComponent));
    }

    private void OnHand(Entity<DiseaseOnCollideComponent> ent, ref DiseaseRelayedEvent<InteractHandEvent> arg)
    {
        if (_timing.CurTime < ent.Comp.Cooldown)
            return;
        ent.Comp.Cooldown = ent.Comp.CooldownInterval + _timing.CurTime;

        if (arg.Args.User == arg.Args.Target)
            return;

        var diseaseUid = Transform(ent.Owner).ParentUid;
        var host = Transform(diseaseUid).ParentUid;
        var target = arg.Args.User;

        if (host == target)
            target = arg.Args.Target;

        if (!TryComp<DiseaseComponent>(diseaseUid, out var diseaseComponent))
            return;

        OnContact(ent, host, target, (diseaseUid, diseaseComponent));
    }

    private void OnHand(Entity<DiseaseOnCollideComponent> ent, ref DiseaseRelayedEvent<BeforeInteractHandEvent> arg)
    {
        if (_timing.CurTime < ent.Comp.Cooldown)
            return;
        ent.Comp.Cooldown = ent.Comp.CooldownInterval + _timing.CurTime;

        var diseaseUid = Transform(ent.Owner).ParentUid;
        var host = Transform(diseaseUid).ParentUid;
        var target = arg.Args.Target;

        if (host == target)
            return;

        if (!TryComp<DiseaseComponent>(diseaseUid, out var diseaseComponent))
            return;

        OnContact(ent, host, target, (diseaseUid, diseaseComponent));
    }

    private void OnContact(Entity<DiseaseOnCollideComponent> ent, EntityUid host, EntityUid target, Entity<DiseaseComponent> disease)
    {
        if (!_whitelist.CheckBoth(target, ent.Comp.Blacklist, ent.Comp.Whitelist))
            return;

        if (disease.Comp.InfectionProgress < ent.Comp.InfectionProgressRequired)
            return;

        var ev = new DiseaseOutgoingSpreadAttemptEvent(
            ent.Comp.SpreadParams.Power,
            ent.Comp.SpreadParams.Chance,
            ent.Comp.SpreadParams.Type
        );
        RaiseLocalEvent(host, ref ev);

        if (ev.Power < 0 || ev.Chance < 0)
            return;

        _disease.DoInfectionAttempt(target, disease.Owner, ev.Power, ev.Chance, ent.Comp.SpreadParams.Type);
    }
}
