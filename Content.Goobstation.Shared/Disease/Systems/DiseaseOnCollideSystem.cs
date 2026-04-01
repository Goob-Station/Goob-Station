using Content.Goobstation.Shared.Disease.Components;
using Content.Shared.Whitelist;
using Robust.Shared.Physics.Events;

namespace Content.Goobstation.Shared.Disease.Systems;

/// <summary>
/// This i used for spreading diseases on collide
/// </summary>
public sealed class DiseaseOnCollideSystem : EntitySystem
{
    [Dependency] private readonly SharedDiseaseSystem _disease = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DiseaseOnCollideComponent, StartCollideEvent>(OnCollide);
    }

    private void OnCollide(Entity<DiseaseOnCollideComponent> ent, ref StartCollideEvent arg)
    {
        var target = arg.OtherEntity;

        if (!_whitelist.CheckBoth(target, ent.Comp.BlackList,ent.Comp.Whitelist))
            return;

        if (ent.Comp.Disease != null)
        {
            _disease.DoInfectionAttempt(target, ent.Comp.Disease.Value, ent.Comp.SpreadParams);
        }
        else
        {
            if (!TryComp<DiseaseCarrierComponent>(ent, out var carrier))
                return;

            foreach (var disease in carrier.Diseases.ContainedEntities)
                _disease.DoInfectionAttempt(target, disease, ent.Comp.SpreadParams);
        }

    }

}
