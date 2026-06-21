using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Random;
using Content.Shared.ActionBlocker;
using Content.Shared.CombatMode;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared._pofitlo.CombatExtended.FightAction.Events;
using Content.Shared._pofitlo.CombatExtended.FightAction.Prototypes;
using Content.Shared.Weapons.Melee;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Throwing;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Stunnable;
using Content.Shared.Random.Helpers;



namespace Content.Shared._pofitlo.CombatExtended.FightAction.AttackStrategySystems;

public abstract class SharedTailAttackSystem : EntitySystem
{

    [Dependency] protected readonly SharedMeleeWeaponSystem MeleeWeaponSystem = default!;
    [Dependency] protected readonly SharedCombatModeSystem CombatMode = default!;
    [Dependency] protected readonly ActionBlockerSystem Blocker = default!;
    [Dependency] protected readonly SharedTransformSystem TransformSystem = default!;
    [Dependency] protected readonly IGameTiming Timing = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeAllEvent<TailAltAttackEvent>(OnTailAltAttackEvent);
        SubscribeAllEvent<TailMainAttackEvent>(OnTailMainAttackEvent);
        //SubscribeAllEvent<DisarmAttackEvent>(OnDisarmAttack);
    }

    private void OnTailMainAttackEvent(TailMainAttackEvent msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { } user)
            return;

        if (!GetTailAsWeapon(user, out var weaponUid, out var weapon, out var fightAction) ||
            weaponUid != GetEntity(msg.Weapon))
            return;


        AttemptAttack(user, weaponUid, weapon, fightAction, msg);

    }

    private void OnTailAltAttackEvent(TailAltAttackEvent msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { } user)
            return;

        if (!GetTailAsWeapon(user, out var weaponUid, out var weapon, out var fightAction) ||
            weaponUid != GetEntity(msg.Weapon))
            return;

        AttemptAttack(user, weaponUid, weapon, fightAction, msg);
    }

    private bool GetTailAsWeapon(EntityUid entity, out EntityUid weaponUid, [NotNullWhen(true)] out MeleeWeaponComponent? melee, [NotNullWhen(true)] out FightActionComponent? fightAction)
    {
        weaponUid = default;
        melee = null;
        fightAction = null;

        if (TryComp(entity, out melee) && TryComp(entity, out fightAction))
        {
            weaponUid = entity;
            return true;
        }

        return false;
    }


    private bool AttemptAttack(EntityUid user, EntityUid weaponUid, MeleeWeaponComponent weapon, FightActionComponent fightAction, AttackEvent attack)
    {
        if (CanAttack(user, weaponUid, weapon))
            return false;

        AdvanceCooldown(weaponUid, weapon, user);
        RaiseAttackEvents(user, weaponUid, weapon, attack);

        weapon.Attacking = true;
        DirtyField(weaponUid, weapon, nameof(MeleeWeaponComponent.Attacking));

        EntProtoId animation; // Goobstation - Edit
        ProtoId<CombatAnimationPrototype>? combatAnimProto;

        switch (attack)
        {
            case TailMainAttackEvent mainAttack:
                DoMainAttack(user, weaponUid, weapon, fightAction, mainAttack);
                animation = fightAction.Animation;
                combatAnimProto = fightAction.CombatAnimationPrototype;
                break;
            case TailAltAttackEvent altAttack:
                DoAltAttack(user, weaponUid, weapon, altAttack);
                animation = fightAction.AltAnimation;
                combatAnimProto = fightAction.AltCombatAnimationPrototype;
                break;
            default:
                return false;
        }

        var spriteRotation = weapon.WideAnimationRotation;
        DoLungeAnimation(user, weaponUid, TransformSystem.ToMapCoordinates(GetCoordinates(attack.Coordinates)), weapon.Range, animation, spriteRotation, weapon.FlipAnimation, combatAnimProto);

        return true;
    }

    private bool CanAttack(EntityUid user, EntityUid weaponUid, MeleeWeaponComponent weapon)
    {
        return weapon.NextAttack > Timing.CurTime ||
               !CombatMode.IsInCombatMode(user) ||
               !Blocker.CanAttack(user, weapon: (weaponUid, weapon));
    }


    private void AdvanceCooldown(EntityUid weaponUid, MeleeWeaponComponent weapon, EntityUid user)
    {
        var curTime = Timing.CurTime;
        var fireRate = TimeSpan.FromSeconds(1f / MeleeWeaponSystem.GetAttackRate(weaponUid, user, weapon));

        weapon.NextAttack = (weapon.NextAttack < curTime ? curTime : weapon.NextAttack) + fireRate;
        DirtyField(weaponUid, weapon, nameof(MeleeWeaponComponent.NextAttack));
    }

    private void RaiseAttackEvents(EntityUid user, EntityUid weaponUid, MeleeWeaponComponent weapon, AttackEvent attack)
    {
        var ev = new AttemptMeleeEvent(user, weaponUid, weapon, attack is HeavyAttackEvent); // Goob edit
        RaiseLocalEvent(weaponUid, ref ev);

        var attackEv = new MeleeAttackEvent(weaponUid);
        RaiseLocalEvent(user, ref attackEv);
    }

    private void DoMainAttack(EntityUid user, EntityUid weaponUid, MeleeWeaponComponent weapon, FightActionComponent fightAction, TailMainAttackEvent mainAttack) // TODO пристроить fightAction
    {
        if (!TryGetAttackDirection(user, TransformSystem.ToMapCoordinates(GetCoordinates(mainAttack.Coordinates)), out var direction))
            return;

        if (mainAttack.Entities == null || mainAttack.Entities.Count <= 0)
            return;

        var targets = CollectTargets(user, mainAttack.Entities);
        if (targets.Count == 0)
            return;

        if (!TryHitTargets(user, weaponUid, weapon, mainAttack, direction, targets))
            return;

        ApplyTargetingEffects(user, targets);
    }

    private bool TryGetAttackDirection(EntityUid user, MapCoordinates targetMap, out Vector2 direction)
    {
        direction = Vector2.Zero;

        if (!TryComp(user, out TransformComponent? userXform))
            return false;

        if (targetMap.MapId != userXform.MapID)
            return false;

        var userPos = TransformSystem.GetWorldPosition(userXform);
        direction = targetMap.Position - userPos;

        return true;
    }

    private List<EntityUid> CollectTargets(EntityUid user, List<NetEntity>? netEntities)
    {
        var targets = new List<EntityUid>();
        if (netEntities == null || netEntities.Count == 0)
            return targets;

        var damageQuery = GetEntityQuery<DamageableComponent>();

        foreach (var entity in GetEntityList(netEntities))
        {
            if (entity == user || !damageQuery.HasComponent(entity))
                continue;

            if (IsHarmCancelled(user, entity)) // Goobstation
                continue;

            targets.Add(entity);
        }

        return targets;
    }
    private bool IsHarmCancelled(EntityUid user, EntityUid target)
    {
        var ev = new BeforeHarmfulActionEvent(user, HarmfulActionType.Harm);
        RaiseLocalEvent(target, ev);
        return ev.Cancelled;
    }

    private bool TryHitTargets(EntityUid user, EntityUid weaponUid, MeleeWeaponComponent weapon,
        TailMainAttackEvent mainAttack, Vector2 direction, List<EntityUid> targets)
    {
        var damage = MeleeWeaponSystem.GetDamage(weaponUid, user, weapon);

        var hitEvent = new MeleeHitEvent(targets, user, weaponUid, damage, direction, GetCoordinates(mainAttack.Coordinates));
        RaiseLocalEvent(weaponUid, hitEvent, true); // Goobstation - broadcast

        if (hitEvent.Handled)
            return false;

        MeleeWeaponSystem.DoSweepingBlow(targets, user, weapon, mainAttack, weaponUid, damage, hitEvent);

        return true;
    }

    private void ApplyTargetingEffects(EntityUid user, List<EntityUid> targets)
    {
        if (!TryComp<TargetingComponent>(user, out var targeting))
            return;

        var target = targeting.Target;

        if (TargetIsLeg(target))
            TryKnockDownTargets(user, targets);

        if (TargetIsChest(target))
            TryShoveTargets(user, targets);
    }
    private bool TargetIsLeg(TargetBodyPart target)
    {
        return (target & TargetBodyPart.Legs) != 0;
    }
    private bool TargetIsChest(TargetBodyPart target)
    {
        return (target & TargetBodyPart.Chest) != 0;
    }

    private void TryKnockDownTargets(EntityUid user, List<EntityUid> targets) //TODO МБ сделать как трай
    {
        foreach (var target in targets)
        {
            if (!TryComp<StaminaComponent>(target, out var targetStamina))
                continue; // Skip if entity doesn't have stamina

            if (!TryPassWithChanceWhichDependsOnStaminaByHyperbola(targetStamina))
                continue;

            var x = TryPassWithChanceWhichDependsOnStaminaByHyperbola(targetStamina);

            TimeSpan knockdownDuration = TimeSpan.FromSeconds(5); // TУДУ. Просто тест.
            _stun.TryKnockdown(target, knockdownDuration, force: true);
        }
    }

    private bool TryPassWithChanceWhichDependsOnStaminaByHyperbola(StaminaComponent stamina, float chanceMultiplier = 10f)
    {
        var staminaLevelInPercent = (1 - (stamina.StaminaDamage / stamina.CritThreshold)) * 100;

        var seed = SharedRandomExtensions.HashCodeCombine(new() { (int)Timing.CurTick.Value, GetNetEntity(stamina.Owner).Id });
        var rand = new System.Random(seed);

        return rand.Prob(Math.Min(1 / staminaLevelInPercent * chanceMultiplier, 1.0f));

        /*
        y = (1/x)*chanceMult, where y is our chamce and x is the percentage of stamina left

        y
        ^
        | *
        | *
        | *
        |
        |  *
        |
        |   *
        |    *
        |     **
        |       ****
        |          *********
        +-----------------------> x

        when chaneMultiplier is 10:
            when stamina is 80% chance is 12.5%
            when stamina is 50% chance is 20%
            when stamina is 20% chance is 50%
            when stamina is 10% chance is 100%

        */
    }



    private void DoAltAttack(EntityUid user, EntityUid weaponUid, MeleeWeaponComponent weapon, TailAltAttackEvent altAttack)
    {
        var damage = MeleeWeaponSystem.GetDamage(weaponUid, user, weapon);

        if (GetEntity(altAttack.Target) is not { } target)
            return;

        TryShoveTargets(user, new List<EntityUid> { target }, -1f);
    }

    private bool TryShoveTargets(EntityUid user, List<EntityUid> targets, float vectorMult = 1f)
    {
        if (targets.Count == 0)
            return false;

        foreach (var target in targets)
        {
            var userPos = TransformSystem.GetWorldPosition(user);
            var targetPos = TransformSystem.GetMapCoordinates(target).Position;
            var direction = targetPos - userPos;

            if (direction == Vector2.Zero)
                continue;

            _throwing.TryThrow(target, direction.Normalized() * vectorMult, 2f, compensateFriction: true);
        }
        return true;
    }
    private void DoLungeAnimation(EntityUid user, EntityUid weapon, MapCoordinates coordinates, float length, string? animation, Angle spriteRotation, bool flipAnimation, ProtoId<CombatAnimationPrototype>? combatAnimProto)
    {
        // TODO: Assert that offset eyes are still okay.
        if (!TryComp(user, out TransformComponent? userXform))
            return;

        var invMatrix = TransformSystem.GetInvWorldMatrix(userXform);
        var localPos = Vector2.Transform(coordinates.Position, invMatrix);

        if (localPos.LengthSquared() <= 0f)
            return;

        localPos = userXform.LocalRotation.RotateVec(localPos);

        // We'll play the effect just short visually so it doesn't look like we should be hitting but actually aren't.
        const float bufferLength = 0.2f;
        var visualLength = length - bufferLength;

        if (localPos.Length() > visualLength)
            localPos = localPos.Normalized() * visualLength;

        DoLunge(user, weapon, localPos, animation, spriteRotation, flipAnimation, combatAnimProto);
    }

    public abstract void DoLunge(EntityUid user, EntityUid weapon, Vector2 localPos, string? animation, Angle spriteRotation, bool flipAnimation, ProtoId<CombatAnimationPrototype>? combatAnimProto, bool predicted = true);
}
