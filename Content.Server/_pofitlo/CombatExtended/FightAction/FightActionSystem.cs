using Content.Shared._pofitlo.CombatExtended.FightAction;
using Content.Shared._pofitlo.CombatExtended.FightAction.Events;
using Content.Shared._pofitlo.CombatExtended.FightAction.Prototypes;
using Content.Shared.Weapons.Melee;
using Robust.Shared.Prototypes;


namespace Content.Server._pofitlo.CombatExtended.FightAction;

public sealed class FightActionSystem : SharedFightActionSystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<FightActionChangeEvent>(OnFightActionChange);
    }

    private void OnFightActionChange(FightActionChangeEvent message, EntitySessionEventArgs args)
    {
        if (!TryComp<FightActionComponent>(GetEntity(message.Uid), out var fightActionComp))
            return;

        if (args.SenderSession.AttachedEntity != GetEntity(message.Uid))
            return; //That help us avoid desync issues, as the client will only send this event for their own entity, but we check just in case.

        fightActionComp.Strategy = message.FightAction;
        fightActionComp.HasHigherPriorityThanWeapons = message.HasHigherPriorityThanWeapons;
        fightActionComp.CombatAnimationPrototype = message.CombatAnimationProto;
        fightActionComp.AltCombatAnimationPrototype = message.AltCombatAnimationProto;
        var uid = GetEntity(message.Uid);
        DirtyField(uid, fightActionComp, nameof(FightActionComponent.Strategy));
        DirtyField(uid, fightActionComp, nameof(FightActionComponent.HasHigherPriorityThanWeapons));
        DirtyField(uid, fightActionComp, nameof(FightActionComponent.CombatAnimationPrototype));
        DirtyField(uid, fightActionComp, nameof(FightActionComponent.AltCombatAnimationPrototype));

        SetMeleeParametersFromPrototypeToComponent(GetEntity(message.Uid), message.FightActionMeleeParametersProto);
    }

    private void SetMeleeParametersFromPrototypeToComponent(EntityUid user, ProtoId<FightActionMeleeParametersPrototype> prototypeId)
    {
        if (!TryComp<MeleeWeaponComponent>(user, out var meleeComp) ||
            !_prototypeManager.TryIndex(prototypeId, out var meleeParametersProto))
            return;

        meleeComp.AltDisarm = meleeParametersProto.HasDisarm;
        DirtyField(user, meleeComp, nameof(MeleeWeaponComponent.AltDisarm));
    }
}
