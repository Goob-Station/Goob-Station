using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Content.Shared.ActionBlocker;
using Content.Shared.CombatMode;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Content.Shared._pofitlo.CombatExtended.FightAction.Events;
using Content.Shared._pofitlo.CombatExtended.FightAction.Prototypes;
using Content.Shared.Weapons.Melee;
using Content.Shared._pofitlo.CombatExtended.FightAction;


using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Common.MartialArts; // Goobstation - Martial Arts
using Content.Shared._EinsteinEngines.Contests;
using Content.Shared._Shitmed.Weapons.Melee.Events; // Shitmed Change
using Content.Shared.ActionBlocker;
using Content.Shared.Actions.Events;
using Content.Shared.Administration.Components;
using Content.Shared.Administration.Logs;
using Content.Shared.CombatMode;
using Content.Shared.Coordinates;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Database;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Hands;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.IdentityManagement;
using Content.Shared.Hands.EntitySystems; // Shitmed Change
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Item;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee.Components;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;


namespace Content.Shared._pofitlo.CombatExtended.FightAction.AttackStrategySystems;

public abstract class SharedTailAttackSystem : EntitySystem
{

    [Dependency] protected readonly SharedMeleeWeaponSystem MeleeWeaponSystem = default!;
    [Dependency] protected readonly SharedCombatModeSystem CombatMode = default!;
    [Dependency] protected readonly ActionBlockerSystem Blocker = default!;
    [Dependency] protected readonly SharedTransformSystem TransformSystem = default!;
    [Dependency] protected readonly IGameTiming Timing = default!;
    //[Dependency] private readonly UseDelaySystem _delay = default!;
    //[Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;

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


        AttemptAttack(user, weaponUid, weapon, fightAction, msg, args.SenderSession);

    }

    private void OnTailAltAttackEvent(TailAltAttackEvent msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { } user)
            return;

        if (!GetTailAsWeapon(user, out var weaponUid, out var weapon, out var fightAction) ||
            weaponUid != GetEntity(msg.Weapon))
            return;

        AttemptAttack(user, weaponUid, weapon, fightAction, msg, args.SenderSession);
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


    private bool AttemptAttack(EntityUid user, EntityUid weaponUid, MeleeWeaponComponent weapon, FightActionComponent fightAction, AttackEvent attack, ICommonSession? session)
    {
        // TODO избавиться от всех комментариев
        var curTime = Timing.CurTime;

        if (weapon.NextAttack > curTime)
            return false;

        if (!CombatMode.IsInCombatMode(user))
            return false;

        EntityUid? target = null;

        if (!Blocker.CanAttack(user, weapon: (weaponUid, weapon)))
            return false;

        // Windup time checked elsewhere.
        var fireRate = TimeSpan.FromSeconds(1f / MeleeWeaponSystem.GetAttackRate(weaponUid, user, weapon));
        var swings = 0;

        // TODO: If we get autoattacks then probably need a shotcounter like guns so we can do timing properly.
        if (weapon.NextAttack < curTime)
            weapon.NextAttack = curTime;

        while (weapon.NextAttack <= curTime)
        {
            weapon.NextAttack += fireRate;
            swings++;
        }

        DirtyField(weaponUid, weapon, nameof(MeleeWeaponComponent.NextAttack));

        // Do this AFTER attack so it doesn't spam every tick
        var ev = new AttemptMeleeEvent(user, weaponUid, weapon, attack is HeavyAttackEvent); // Goob edit
        RaiseLocalEvent(weaponUid, ref ev);

        //if (!MeleeWeaponSystem.DoHeavyAttack(user, heavy, weaponUid, weapon, session))
            //return false;

        EntProtoId animation; // Goobstation - Edit
        var spriteRotation = weapon.AnimationRotation;

        var attackEv = new MeleeAttackEvent(weaponUid);
        RaiseLocalEvent(user, ref attackEv);

        weapon.Attacking = true;
        DirtyField(weaponUid, weapon, nameof(MeleeWeaponComponent.Attacking));

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

        spriteRotation = weapon.WideAnimationRotation;
        DoLungeAnimation(user, weaponUid, TransformSystem.ToMapCoordinates(GetCoordinates(attack.Coordinates)), weapon.Range, animation, spriteRotation, weapon.FlipAnimation, combatAnimProto);

        // TODO слишком раздутая система. Надо будет сократить
        return true;
    }

    private void DoMainAttack(EntityUid user, EntityUid weaponUid, MeleeWeaponComponent weapon, FightActionComponent fightAction, TailMainAttackEvent mainAttack) // TODO пристроить fightAction
    {
        if (!TryComp(user, out TransformComponent? userXform))
            return;

        var targetMap = TransformSystem.ToMapCoordinates(GetCoordinates(mainAttack.Coordinates));

        if (targetMap.MapId != userXform.MapID)
            return;

        var userPos = TransformSystem.GetWorldPosition(userXform);
        var direction = targetMap.Position - userPos;

        if (mainAttack.Entities == null || mainAttack.Entities.Count <= 0)
            return;

        var entities = GetEntityList(mainAttack.Entities);
        var targets = new List<EntityUid>();
        var damageQuery = GetEntityQuery<DamageableComponent>();

        foreach (var entity in entities)
        {
            if (entity == user ||
                !damageQuery.HasComponent(entity))
                continue;

            // Goobstation start
            var beforeEvent = new BeforeHarmfulActionEvent(user, HarmfulActionType.Harm);
            RaiseLocalEvent(entity, beforeEvent);
            if (beforeEvent.Cancelled)
                continue;
            // Goobstation end

            targets.Add(entity);
        }

        var damage = MeleeWeaponSystem.GetDamage(weaponUid, user, weapon);

        var hitEvent = new MeleeHitEvent(targets, user, weaponUid, damage, direction, GetCoordinates(mainAttack.Coordinates)); // Goob edit
        RaiseLocalEvent(weaponUid, hitEvent, true); // Goob station - broadcast

        if (hitEvent.Handled)
            return;

        MeleeWeaponSystem.DoSweepingBlow(targets, user, weapon, mainAttack, weaponUid, damage, hitEvent);
    } // TODO ахуй какие большие функции

    private void DoAltAttack(EntityUid user, EntityUid weaponUid, MeleeWeaponComponent weapon, TailAltAttackEvent altAttack)
    {
        var damage = MeleeWeaponSystem.GetDamage(weaponUid, user, weapon);

        if (GetEntity(altAttack.Target) is not { } target)
            return;

        var userPos = TransformSystem.GetWorldPosition(user);
        var targetPos = TransformSystem.GetMapCoordinates(target).Position;
        var direction = targetPos - userPos;

        if (direction == Vector2.Zero)
            return;

        _throwing.TryThrow(target, direction.Normalized() * -1f, 2f, compensateFriction: true);
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
