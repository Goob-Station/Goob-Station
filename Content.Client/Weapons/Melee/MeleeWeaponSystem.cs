using System.Linq;
using System.Numerics;
using Content.Client.Gameplay;
using Content.Shared._Goobstation.Weapons.MeleeDash;
using Content.Shared._White.Blink;
using Content.Shared.CombatMode;
using Content.Shared.Effects;
using Content.Shared.Hands.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.StatusEffect;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Wieldable.Components;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Client.State;
using Robust.Shared.Input;
using Robust.Shared.Map;
using Robust.Shared.Player;

namespace Content.Client.Weapons.Melee;

public sealed partial class MeleeWeaponSystem : SharedMeleeWeaponSystem
{
    [Dependency] private readonly IEyeManager _eyeManager = default!;
    [Dependency] private readonly IInputManager _inputManager = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IStateManager _stateManager = default!;
    [Dependency] private readonly AnimationPlayerSystem _animation = default!;
    [Dependency] private readonly InputSystem _inputSystem = default!;
    [Dependency] private readonly SharedColorFlashEffectSystem _color = default!;
    [Dependency] private readonly MapSystem _map = default!;
    [Dependency] private readonly TransformSystem _transform = default!; // Goobstation

    private EntityQuery<TransformComponent> _xformQuery;

    private const string MeleeLungeKey = "melee-lunge";

    public override void Initialize()
    {
        base.Initialize();
        _xformQuery = GetEntityQuery<TransformComponent>();
        SubscribeNetworkEvent<MeleeLungeEvent>(OnMeleeLunge);
        UpdatesOutsidePrediction = true;
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);
        UpdateEffects();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!Timing.IsFirstTimePredicted)
            return;

        var entityNull = _player.LocalEntity;

        if (entityNull == null)
            return;

        var entity = entityNull.Value;

        if (!TryGetWeapon(entity, out var weaponUid, out var weapon))
            return;

        if (!CombatMode.IsInCombatMode(entity) || !Blocker.CanAttack(entity, weapon: (weaponUid, weapon)))
        {
            weapon.Attacking = false;
            return;
        }

        var useDown = _inputSystem.CmdStates.GetState(EngineKeyFunctions.Use);
        var altDown = _inputSystem.CmdStates.GetState(EngineKeyFunctions.UseSecondary);

        if (weapon.AutoAttack || useDown != BoundKeyState.Down && altDown != BoundKeyState.Down)
        {
            if (weapon.Attacking)
            {
                RaisePredictiveEvent(new StopAttackEvent(GetNetEntity(weaponUid)));
            }
        }

        if (weapon.Attacking || weapon.NextAttack > Timing.CurTime)
        {
            return;
        }

        // TODO using targeted actions while combat mode is enabled should NOT trigger attacks.

        // TODO: Need to make alt-fire melee its own component I guess?
        // Melee and guns share a lot in the middle but share virtually nothing at the start and end so
        // it's kinda tricky.
        // I think as long as we make secondaries their own component it's probably fine
        // as long as guncomp has an alt-use key then it shouldn't be too much of a PITA to deal with.

        //Frontier: better support melee vs. ranged checks
        /*if (TryComp<GunComponent>(weaponUid, out var gun) && gun.UseKey)
        {
            return;
        }*/

        // Ranged component has priority over melee if both are supported.
        bool gunBoundToUse = false;
        bool gunBoundToAlt = false;
        if (TryComp<GunComponent>(weaponUid, out var gun)) {
            gunBoundToUse = gun.UseKey;
            gunBoundToAlt = !gun.UseKey; //Bound to alt-use when false

            // If ranged mode only works when wielded, do not block melee attacks when unwielded
            // (e.g. crusher & crusher glaive)
            if (TryComp<GunRequiresWieldComponent>(weaponUid, out var _) &&
                    TryComp<WieldableComponent>(weaponUid, out var wield)) {
                gunBoundToUse &= wield.Wielded;
                gunBoundToAlt &= wield.Wielded;
            }
        }
        //End Frontier

        var mousePos = _eyeManager.PixelToMap(_inputManager.MouseScreenPosition);

        if (mousePos.MapId == MapId.Nullspace)
        {
            return;
        }

        EntityCoordinates coordinates;

        if (MapManager.TryFindGridAt(mousePos, out var gridUid, out _))
        {
            coordinates = TransformSystem.ToCoordinates(gridUid, mousePos);
        }
        else
        {
            coordinates = TransformSystem.ToCoordinates(_map.GetMap(mousePos.MapId), mousePos);
        }

        // Heavy attack.
        if (altDown == BoundKeyState.Down && !gunBoundToAlt) //Frontier: add !gunBoundToAlt condition
        {
            // If it's an unarmed attack then do a disarm
            if (weapon.AltDisarm && weaponUid == entity)
            {
                EntityUid? target = null;

                if (_stateManager.CurrentState is GameplayStateBase screen)
                {
                    target = screen.GetDamageableClickedEntity(mousePos); // Goob edit
                }

                EntityManager.RaisePredictiveEvent(new DisarmAttackEvent(GetNetEntity(target), GetNetCoordinates(coordinates)));
                return;
            }

            // WD EDIT START
            if (TryComp(weaponUid, out BlinkComponent? blink) && blink.IsActive)
            {
                var direction = GetDirection();
                if (direction != Vector2.Zero)
                    RaisePredictiveEvent(new BlinkEvent(GetNetEntity(weaponUid), direction));
                return;
            }
            // WD EDIT END

            // Goobstation
            if (TryComp(weaponUid, out MeleeDashComponent? dash))
            {
                var direction = GetDirection();
                if (direction != Vector2.Zero)
                    RaisePredictiveEvent(new MeleeDashEvent(GetNetEntity(weaponUid), direction));
                return;
            }

            ClientHeavyAttack(entity, coordinates, weaponUid, weapon);
            return;

            // Goobstation
            Vector2 GetDirection()
            {
                if (!_xformQuery.TryGetComponent(entity, out var userXform))
                    return Vector2.Zero;

                var targetMap = _transform.ToMapCoordinates(coordinates);

                if (targetMap.MapId != userXform.MapID)
                    return Vector2.Zero;

                var userPos = TransformSystem.GetWorldPosition(userXform);
                return targetMap.Position - userPos;
            }
        }

        // Light attack
        if (useDown == BoundKeyState.Down && !gunBoundToUse) //Frontier: add !gunBoundToUse condition
        {
            var attackerPos = TransformSystem.GetMapCoordinates(entity);

            if (mousePos.MapId != attackerPos.MapId ||
                (attackerPos.Position - mousePos.Position).Length() > weapon.Range)
            {
                return;
            }

            EntityUid? target = null;

            if (_stateManager.CurrentState is GameplayStateBase screen)
            {
                target = screen.GetDamageableClickedEntity(mousePos); // Goob edit
            }

            // Don't light-attack if interaction will be handling this instead
            if (Interaction.CombatModeCanHandInteract(entity, target))
                return;

            RaisePredictiveEvent(new LightAttackEvent(GetNetEntity(target), GetNetEntity(weaponUid), GetNetCoordinates(coordinates)));
        }
    }

    protected override bool InRange(EntityUid user, EntityUid target, float range, ICommonSession? session)
    {
        var xform = Transform(target);
        var targetCoordinates = xform.Coordinates;
        var targetLocalAngle = xform.LocalRotation;

        return Interaction.InRangeUnobstructed(user, target, targetCoordinates, targetLocalAngle, range);
    }

    protected override void DoDamageEffect(List<EntityUid> targets, EntityUid? user, TransformComponent targetXform)
    {
        // Server never sends the event to us for predictiveeevent.
        _color.RaiseEffect(Color.Red, targets, Filter.Local());
    }

    protected override bool DoDisarm(EntityUid user, DisarmAttackEvent ev, EntityUid meleeUid, MeleeWeaponComponent component, ICommonSession? session)
    {
        if (!base.DoDisarm(user, ev, meleeUid, component, session))
            return false;

        if (!TryComp<CombatModeComponent>(user, out var combatMode) ||
            combatMode.CanDisarm != true)
        {
            return false;
        }

        var target = GetEntity(ev.Target);

        // They need to either have hands...
        if (!HasComp<HandsComponent>(target!.Value))
        {
            // or just be able to be shoved over.
            if (TryComp<StatusEffectsComponent>(target, out var status) && status.AllowedEffects.Contains("KnockedDown"))
                return true;

            if (Timing.IsFirstTimePredicted && HasComp<MobStateComponent>(target.Value))
                PopupSystem.PopupEntity(Loc.GetString("disarm-action-disarmable", ("targetName", target.Value)), target.Value);

            return false;
        }

        return true;
    }

    /// <summary>
    /// Raises a heavy attack event with the relevant attacked entities.
    /// This is to avoid lag effecting the client's perspective too much.
    /// </summary>
    private void ClientHeavyAttack(EntityUid user, EntityCoordinates coordinates, EntityUid meleeUid, MeleeWeaponComponent component)
    {
        // Only run on first prediction to avoid the potential raycast entities changing.
        if (!_xformQuery.TryGetComponent(user, out var userXform) ||
            !Timing.IsFirstTimePredicted)
        {
            return;
        }

        var targetMap = TransformSystem.ToMapCoordinates(coordinates);

        if (targetMap.MapId != userXform.MapID)
            return;

        var userPos = TransformSystem.GetWorldPosition(userXform);
        var direction = targetMap.Position - userPos;
        var distance = MathF.Min(component.Range, direction.Length());

        // This should really be improved. GetEntitiesInArc uses pos instead of bounding boxes.
        // Server will validate it with InRangeUnobstructed.
        var entities = GetNetEntityList(ArcRayCast(userPos, direction.ToWorldAngle(), component.Angle, distance, userXform.MapID, user).ToList());
        RaisePredictiveEvent(new HeavyAttackEvent(GetNetEntity(meleeUid), entities.GetRange(0, Math.Min(MaxTargets, entities.Count)), GetNetCoordinates(coordinates)));
    }

    private void OnMeleeLunge(MeleeLungeEvent ev)
    {
        var ent = GetEntity(ev.Entity);
        var entWeapon = GetEntity(ev.Weapon);

        // Entity might not have been sent by PVS.
        if (Exists(ent) && Exists(entWeapon))
            DoLunge(ent, entWeapon, ev.Angle, ev.LocalPos, ev.Animation);
    }
}
