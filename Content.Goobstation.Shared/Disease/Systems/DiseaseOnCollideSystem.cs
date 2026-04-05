using Content.Goobstation.Shared.Disease.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Whitelist;
using Robust.Shared.Physics.Events;

namespace Content.Goobstation.Shared.Disease.Systems;

/// <summary>
/// This is used for spreading diseases on collide
/// </summary>
public sealed class DiseaseOnCollideSystem : EntitySystem
{
    [Dependency] private readonly SharedDiseaseSystem _disease = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly MobStateSystem _mobstate = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DiseaseOnCollideComponent, DiseaseRelayedEvent<StartCollideEvent>>(OnCollide);
    }

    private void OnStartup(Entity<DiseaseOnCollideComponent> ent, ComponentStartup arg)
    {
        if (ent.Comp.Disease is not null)
            return;
        if (!TryComp<DiseaseComponent>(ent.Owner, out var disease))
            return;

        var proto = MetaData(ent).EntityPrototype;
        if (proto == null)
            return;

        ent.Comp.Disease = proto.ID;
    }

    private void OnCollide(Entity<DiseaseOnCollideComponent> ent, ref DiseaseRelayedEvent<StartCollideEvent> arg)
    {
        var host = arg.Args.OurEntity;
        var target = arg.Args.OtherEntity;

        if (!_mobstate.IsDead(host)  && ent.Comp.OnlyIfDead)
            return;

        if (!_whitelist.CheckBoth(target, ent.Comp.BlackList,ent.Comp.Whitelist))
            return;

        if (ent.Comp.Disease != null)
        {
            _disease.DoInfectionAttempt(target, ent.Comp.Disease.Value, ent.Comp.SpreadParams);
        }
        else if(TryComp<DiseaseCarrierComponent>(host, out var carrier))
        {
                foreach (var disease in carrier.Diseases.ContainedEntities)
                    _disease.DoInfectionAttempt(target, disease, ent.Comp.SpreadParams);
        }
    }

}
