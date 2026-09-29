using Content.Goobstation.Shared.Disease.Components;  
using Content.Server.GameTicking;  
using Content.Server.GameTicking.Rules;  
using Content.Shared.GameTicking.Components;  
using Content.Shared.Mobs.Components;  
using Content.Shared.Mobs.Systems;  
using Content.Shared.GameTicking;  
  
namespace Content.Goobstation.Server.Disease;  
  
public sealed partial class DiseaseSelectionSystem : GameRuleSystem<DiseaseSelectionComponent>  
{  
    [Dependency] private readonly DiseaseSystem _disease = default!;  
    [Dependency] private readonly MobStateSystem _mobState = default!;  
  
    protected override void Started(EntityUid uid, DiseaseSelectionComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)  
    {  
        if (GameTicker.RunLevel != GameRunLevel.InRound || component.Disease == null)  
            return;  
  
        List<EntityUid> aliveList = new();  
        var query = EntityQueryEnumerator<DiseaseCarrierComponent, MobStateComponent, HumanoidAppearanceComponent>();  
        while (query.MoveNext(out var target, out _, out var mobState))  
        {  
            if (!_mobState.IsDead(target, mobState))  
                aliveList.Add(target);  
        }  
        RobustRandom.Shuffle(aliveList);  
  
        var toInfect = RobustRandom.Next(component.Min, component.Max + 1);  
        for (var i = 0; i < toInfect && i < aliveList.Count; i++)  
        {  
            _disease.DoInfectionAttempt(aliveList[i], component.Disease.Value, component.SpreadParams);  
        }  
    }  
}