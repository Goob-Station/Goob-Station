using Robust.Shared.Map;
using Content.Shared._pofitlo.CombatExtended.FightAction;
using Content.Shared.Weapons.Melee;

namespace Content.Client._pofitlo.CombatExtended.AttackStrategies;

public interface IAttackStrategy
{
    void ExecuteMainAttack(EntityUid attacker, MapCoordinates mousePos, EntityCoordinates coordinates, EntityUid weaponUid, MeleeWeaponComponent meleeComponent);

    void ExecuteAltAttack(EntityUid attacker, EntityCoordinates coordinates, EntityUid weaponUid, MeleeWeaponComponent meleeComponent);

    void ExecuteDisarm(EntityUid attacker, MapCoordinates mousePos, EntityCoordinates coordinates);
}
