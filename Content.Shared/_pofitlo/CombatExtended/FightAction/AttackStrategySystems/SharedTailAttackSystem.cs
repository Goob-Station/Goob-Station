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
using Content.Shared.Weapons.Melee;
using Content.Shared._pofitlo.CombatExtended.FightAction;


namespace Content.Shared._pofitlo.CombatExtended.FightAction.AttackStrategySystems;

public abstract class SharedTailAttackSystem : EntitySystem
{

    [Dependency] protected readonly SharedMeleeWeaponSystem MeleeWeaponSystem = default!;
    [Dependency] protected readonly SharedCombatModeSystem CombatMode = default!;
    [Dependency] protected readonly ActionBlockerSystem Blocker = default!;
    [Dependency] protected readonly SharedTransformSystem TransformSystem = default!;
    [Dependency] protected readonly IGameTiming Timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        //SubscribeAllEvent<HeavyAttackEvent>(OnHeavyAttack);
        SubscribeAllEvent<TailLightAttackEvent>(OnTailLightAttackEvent);
        //SubscribeAllEvent<DisarmAttackEvent>(OnDisarmAttack);
    }

    private void OnTailLightAttackEvent(TailLightAttackEvent msg, EntitySessionEventArgs args)
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
        var ev = new AttemptMeleeEvent(user, weaponUid, weapon); // Lavaland Change: WHY ARENT YOU FUCKS PASSING THE USER RAHHHHHHHHHHH
        RaiseLocalEvent(weaponUid, ref ev);

        //if (!MeleeWeaponSystem.DoHeavyAttack(user, heavy, weaponUid, weapon, session))
            //return false;

        EntProtoId animation; // Goobstation - Edit
        var spriteRotation = weapon.AnimationRotation;

        animation = fightAction.Animation;
        spriteRotation = weapon.WideAnimationRotation;
        DoLungeAnimation(user, weaponUid, TransformSystem.ToMapCoordinates(GetCoordinates(attack.Coordinates)), weapon.Range, animation, spriteRotation, weapon.FlipAnimation); // Goobstation - Edit

        var attackEv = new MeleeAttackEvent(weaponUid);
        RaiseLocalEvent(user, ref attackEv);

        weapon.Attacking = true;
        DirtyField(weaponUid, weapon, nameof(MeleeWeaponComponent.Attacking));
        return true;
    }

    private void DoLungeAnimation(EntityUid user, EntityUid weapon, MapCoordinates coordinates, float length, string? animation, Angle spriteRotation, bool flipAnimation)
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

        DoLunge(user, weapon, localPos, animation, spriteRotation, flipAnimation);
    }

    public abstract void DoLunge(EntityUid user, EntityUid weapon, Vector2 localPos, string? animation, Angle spriteRotation, bool flipAnimation, bool predicted = true);
}
