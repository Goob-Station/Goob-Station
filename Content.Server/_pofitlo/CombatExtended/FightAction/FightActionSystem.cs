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

        fightActionComp.Strategy = message.FightAction;
        fightActionComp.HasHigherPriorityThanWeapons = message.HasHigherPriorityThanWeapons;
        fightActionComp.CombatAnimationPrototype = message.CombatAnimationProto;
        DirtyField(GetEntity(message.Uid), fightActionComp, nameof(FightActionComponent.Strategy));
        DirtyField(GetEntity(message.Uid), fightActionComp, nameof(FightActionComponent.HasHigherPriorityThanWeapons));
        DirtyField(GetEntity(message.Uid), fightActionComp, nameof(FightActionComponent.CombatAnimationPrototype));

        SetMeleeParametersFromPrototypeToComponent(GetEntity(message.Uid), message.FightActionMeleeParametersProto);
    }

    private void SetMeleeParametersFromPrototypeToComponent(EntityUid user, ProtoId<FightActionMeleeParametersPrototype> prototypeId)
    {
        if (!TryComp<MeleeWeaponComponent>(user, out var meleeComp))
            return;

        if (_prototypeManager.TryIndex(prototypeId, out var meleeParametersProto))

            meleeComp.AltDisarm = meleeParametersProto.HasDisarm;
    }
}
