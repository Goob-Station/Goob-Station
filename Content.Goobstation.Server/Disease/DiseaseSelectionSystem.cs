using Content.Goobstation.Shared.Disease.Components;  
using Content.Server.GameTicking;  
using Content.Server.GameTicking.Rules;  
using Content.Shared.GameTicking.Components;  
using Content.Shared.Mobs.Components;  
using Content.Shared.Mobs.Systems;  
using Content.Shared.GameTicking;  
using Content.Shared.Humanoid;
using Content.Shared.Mind;
using Content.Shared.Roles;

namespace Content.Goobstation.Server.Disease;  
  
public sealed partial class DiseaseSelectionSystem : GameRuleSystem<DiseaseSelectionComponent>  
{  
    [Dependency] private readonly DiseaseSystem _disease = default!;  
    [Dependency] private readonly MobStateSystem _mobState = default!;  
    [Dependency] private readonly SharedMindSystem _mind = default!;  
    [Dependency] private readonly SharedRoleSystem _role = default!;
  
    protected override void Started(EntityUid uid, DiseaseSelectionComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)  
    {  
        if (GameTicker.RunLevel != GameRunLevel.InRound || component.Disease == null)  
            return;  
  
        List<EntityUid> targetList = new();  
        var query = EntityQueryEnumerator<DiseaseCarrierComponent, MobStateComponent, HumanoidAppearanceComponent>();  
        while (query.MoveNext(out var target, out _, out var mobState, out _))  
        {  
            if (_mobState.IsDead(target, mobState))  
                continue;

            if (!_mind.TryGetMind(target, out var mindId, out _) || _role.MindIsAntagonist(mindId))  
                continue;

            targetList.Add(target);  
        }  
        RobustRandom.Shuffle(targetList);  
  
        var toInfect = RobustRandom.Next(component.Min, component.Max + 1);  
        for (var i = 0; i < toInfect && i < targetList.Count; i++)  
        {  
            _disease.DoInfectionAttempt(targetList[i], component.Disease.Value, component.SpreadParams);  
        }  
    }  
}