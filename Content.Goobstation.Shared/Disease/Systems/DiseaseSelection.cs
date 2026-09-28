using Content.Goobstation.Shared.Disease.Components;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Events;
using Content.Server.GameTicking.Rules;
using Content.Goobstation.Shared.MisandryBox.Thunderdome;
using Robust.Shared.Random;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Disease.Systems;

public sealed partial class DiseaseSelection : EntitySystem
{
    [Dependency] private readonly SharedDiseaseSystem _disease = default!;

    public override void Initialize()
    {
        base.Initialize();

    }

    private void OnMeleeHit(Entity<DiseaseOnHitComponent> ent, ref MeleeHitEvent args)
    {
        if (!args.IsHit)
            return;

        foreach (var target in args.HitEntities)
        {
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
    protected override void Started(EntityUid uid, DiseaseSelectionComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        if (GameTicker.RunLevel != GameRunLevel.InRound)
            return;
        
        List<DiseaseCarrierComponent> aliveList = new();

        foreach (var (carrier, mobState) in EntityManager.EntityQuery<DiseaseCarrierComponent, MobStateComponent>())
        {
            if (!mobState.IsDead())
                aliveList.Add(carrier);
        }
        RobustRandom.Shuffle(aliveList);

        var toInfect = RobustRandom.Next(ent.Comp.Min, ent.Comp.Max);

        foreach (var target in aliveList)
        {
            if (toInfect-- == 0)
                break;

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
}
