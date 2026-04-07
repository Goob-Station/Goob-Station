using Content.Goobstation.Shared.Disease.Components;
using Content.Shared.Whitelist;
using Robust.Shared.Containers;
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
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DiseaseOnCollideComponent, DiseaseRelayedEvent<StartCollideEvent>>(OnCollide);
    }

    private void OnCollide(Entity<DiseaseOnCollideComponent> ent, ref DiseaseRelayedEvent<StartCollideEvent> arg)
    {
        if (_timing.CurTime < ent.Comp.Cooldown)
            return;

        var host = arg.Args.OurEntity;
        var target = arg.Args.OtherEntity;

        if (!_whitelist.CheckBoth(target, ent.Comp.Blacklist, ent.Comp.Whitelist))
            return;

        var ev = new DiseaseOutgoingSpreadAttemptEvent(
            ent.Comp.SpreadParams.Power,
            ent.Comp.SpreadParams.Chance,
            ent.Comp.SpreadParams.Type
        );
        RaiseLocalEvent(host, ref ev);

        if (ev.Power < 0 || ev.Chance < 0)
            return;

        if (ent.Comp.Disease != null)
            _disease.DoInfectionAttempt(target, ent.Comp.Disease.Value, ev.Power, ev.Chance, ent.Comp.SpreadParams.Type);
        else  if (_container.TryGetContainingContainer(ent.Owner, out var container))
            _disease.DoInfectionAttempt(target, container.Owner,  ev.Power, ev.Chance, ent.Comp.SpreadParams.Type);

        ent.Comp.Cooldown = ent.Comp.CooldownInterval + _timing.CurTime;
    }
}
