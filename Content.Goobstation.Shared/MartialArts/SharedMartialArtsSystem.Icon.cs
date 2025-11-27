using Content.Goobstation.Common.MartialArts;
using Content.Goobstation.Shared.MartialArts.Components;
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared.Weapons.Reflect;

namespace Content.Goobstation.Shared.MartialArts;

public partial class SharedMartialArtsSystem
{
    private void InitializeIcon()
    {
        SubscribeLocalEvent<MartialArtsAlertComponent, ComponentInit>(OnCompInitAlert);
        SubscribeLocalEvent<MartialArtsAlertComponent, ToggleMartialArtsStanceEvent>(OnToggleStanceMode);
        SubscribeLocalEvent<MartialArtsKnowledgeComponent,  ComponentInit>(OnCompInitKnowledge);  
    }

    private void OnToggleStanceMode(Entity<MartialArtsAlertComponent> ent, ref ToggleMartialArtsStanceEvent args)
    {
        if (args.Handled)
            return;
        var martial = Comp<MartialArtsKnowledgeComponent>(ent);
        // stupid fucking common and shared shit is ass
        ent.Comp.Stance = !ent.Comp.Stance; //flip boolean
        martial.Stance = !martial.Stance; //flip boolean
        
        _alerts.ShowAlert(ent.Owner, ent.Comp.MartialArtProtoId, (short)(ent.Comp.Stance ? 1 : 0));
        DirtyField(ent.AsNullable(), nameof(ent.Comp.Stance), null);
        if (martial.Stance)
        {
            StanceOn(ent, args);
        }
        else
        {
            StanceOff(ent, args);
        }

        args.Handled = true;
    }
    
    #region Event Methods
    private void OnCompInitKnowledge(Entity<MartialArtsKnowledgeComponent> ent, ref ComponentInit args)
    {
        if (TryComp<MartialArtsKnowledgeComponent>(ent, out var knowledge) &&
            (knowledge.MartialArtsForm == MartialArtsForms.SleepingCarp ||
             knowledge.MartialArtsForm == MartialArtsForms.CloseQuartersCombat))
            EnsureComp<MartialArtsAlertComponent>(ent);
    }

    private void OnCompInitAlert(Entity<MartialArtsAlertComponent> ent, ref ComponentInit args)
    {
        if (TryComp<MartialArtsKnowledgeComponent>(ent, out var knowledge) &&
            (knowledge.MartialArtsForm == MartialArtsForms.SleepingCarp ||
             knowledge.MartialArtsForm == MartialArtsForms.CloseQuartersCombat))
            _alerts.ShowAlert(ent, ent.Comp.MartialArtProtoId, 0);
    }
    #endregion Event Methods
    
    #region Helper Methods
    private void StanceOn(Entity<MartialArtsAlertComponent> ent, ToggleMartialArtsStanceEvent args)
    {
        _popupSystem.PopupPredicted(Loc.GetString("martial-arts-action-toggle-stance-mode-on"), ent.Owner,
            args.User);
        if (TryComp<SleepingCarpStudentComponent>(ent.Owner, out var studentComp) && studentComp.Stage >= 3)
        {
            var userReflect = EnsureComp<ReflectComponent>(ent.Owner);
            userReflect.Examinable = false; // no doxxing scarp users by examining lmao
            userReflect.ReflectProb = 1;
            userReflect.Spread = 60;
            Dirty(ent.Owner, userReflect);
        }
    }

    private void StanceOff(Entity<MartialArtsAlertComponent> ent, ToggleMartialArtsStanceEvent args)
    {
        _popupSystem.PopupPredicted(Loc.GetString("martial-arts-action-toggle-stance-mode-off"), ent.Owner,
            args.User);
        if (TryComp<SleepingCarpStudentComponent>(ent.Owner, out var studentComp) && studentComp.Stage >= 3)
            RemComp<ReflectComponent>(ent.Owner);
    }
    #endregion Helper Methods
}
