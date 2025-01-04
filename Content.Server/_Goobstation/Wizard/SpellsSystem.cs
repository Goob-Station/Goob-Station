using System.Numerics;
using Content.Server.Abilities.Mime;
using Content.Server.Antag;
using Content.Server.Chat.Systems;
using Content.Server.Emp;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Fluids.EntitySystems;
using Content.Server.Inventory;
using Content.Server.Polymorph.Systems;
using Content.Server.Singularity.EntitySystems;
using Content.Server.Spreader;
using Content.Server.Weapons.Ranged.Systems;
using Content.Shared._Goobstation.Wizard;
using Content.Shared._Goobstation.Wizard.BindSoul;
using Content.Shared._Goobstation.Wizard.SpellCards;
using Content.Shared.Chemistry.Components;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.Gibbing.Events;
using Content.Shared.Humanoid;
using Content.Shared.Maps;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Goobstation.Wizard;

public sealed class SpellsSystem : SharedSpellsSystem
{
    [Dependency] private readonly EmpSystem _emp = default!;
    [Dependency] private readonly SmokeSystem _smoke = default!;
    [Dependency] private readonly SpreaderSystem _spreader = default!;
    [Dependency] private readonly GravityWellSystem _gravityWell = default!;
    [Dependency] private readonly ExplosionSystem _explosion = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly ServerInventorySystem _inventory = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly GunSystem _gun = default!;

    protected override void MakeMime(EntityUid uid)
    {
        base.MakeMime(uid);

        EnsureComp<MimePowersComponent>(uid).CanBreakVow = false;
    }

    protected override void Emp(DisableTechEvent ev)
    {
        base.Emp(ev);

        // This doesn't invoke EmpPulse() because I don't want it to spawn emp effect and play pulse sound
        var coords = TransformSystem.GetMapCoordinates(ev.Performer);
        foreach (var uid in Lookup.GetEntitiesInRange(coords, ev.Range))
        {
            _emp.TryEmpEffects(uid, ev.EnergyConsumption, ev.DisableDuration);
        }
    }

    protected override void SpawnSmoke(SmokeSpellEvent ev)
    {
        base.SpawnSmoke(ev);

        var xform = Transform(ev.Performer);
        var mapCoords = TransformSystem.GetMapCoordinates(ev.Performer, xform);

        if (!MapManager.TryFindGridAt(mapCoords, out var gridUid, out var grid) ||
            !Map.TryGetTileRef(gridUid, grid, xform.Coordinates, out var tileRef) ||
            tileRef.Tile.IsEmpty)
            return;

        if (_spreader.RequiresFloorToSpread(ev.Proto.ToString()) && tileRef.Tile.IsSpace())
            return;

        var coords = Map.MapToGrid(gridUid, mapCoords);
        var ent = Spawn(ev.Proto, coords.SnapToGrid());
        if (!TryComp<SmokeComponent>(ent, out var smoke))
        {
            Log.Error($"Smoke prototype {ev.Proto} was missing SmokeComponent");
            Del(ent);
            return;
        }

        _smoke.StartSmoke(ent, new Solution(ev.Solution), ev.Duration, ev.SpreadAmount, smoke);
    }

    protected override void Repulse(RepulseEvent ev)
    {
        var mapPos = TransformSystem.GetMapCoordinates(ev.Performer);

        if (mapPos == MapCoordinates.Nullspace)
            return;

        var baseMatrixDeltaV = new Matrix3x2(-ev.Force, 0f, 0f, -ev.Force, 0f, 0f);
        var epicenter = mapPos.Position;
        var minRange2 = ev.MinRange * ev.MinRange;
        var xformQuery = GetEntityQuery<TransformComponent>();

        foreach (var (entity, physics) in Lookup.GetEntitiesInRange<PhysicsComponent>(mapPos,
                     ev.MaxRange,
                     flags: LookupFlags.Dynamic | LookupFlags.Sundries))
        {
            if (physics.BodyType == BodyType.Static)
                continue;

            if (entity == ev.Performer)
                continue;

            if (!_gravityWell.CanGravPulseAffect(entity))
                continue;

            var xform = xformQuery.Comp(entity);

            var displacement = epicenter - TransformSystem.GetWorldPosition(xform, xformQuery);
            var distance2 = displacement.LengthSquared();
            if (distance2 < minRange2)
                continue;

            Stun.TryParalyze(entity, ev.StunTime, true);

            if (ev.EffectProto != null)
                Spawn(ev.EffectProto.Value, TransformSystem.GetMapCoordinates(entity, xform));

            var scaling = (1f / distance2) * physics.Mass;
            Physics.ApplyLinearImpulse(entity,
                Vector2.TransformNormal(displacement, baseMatrixDeltaV) * scaling,
                body: physics);
        }
    }

    protected override void ExplodeCorpse(CorpseExplosionEvent ev)
    {
        base.ExplodeCorpse(ev);

        _explosion.QueueExplosion(ev.Target,
            ev.ExplosionId,
            ev.TotalIntensity,
            ev.Slope,
            ev.MaxIntenity,
            0f,
            0,
            false,
            ev.Performer);
    }

    protected override void Emote(EntityUid uid, string emoteId)
    {
        base.Emote(uid, emoteId);

        _chat.TryEmoteWithChat(uid, emoteId);
    }

    protected override void BindSoul(BindSoulEvent ev, EntityUid item, EntityUid mind, MindComponent mindComponent)
    {
        base.BindSoul(ev, item, mind, mindComponent);

        var xform = Transform(ev.Performer);
        var meta = MetaData(ev.Performer);

        var mapId = xform.MapUid;

        var newEntity = Spawn(ev.Entity,
            TransformSystem.GetMapCoordinates(ev.Performer, xform),
            rotation: TransformSystem.GetWorldRotation(ev.Performer));

        if (Container.TryGetContainingContainer((ev.Performer, xform, meta), out var cont))
            Container.Insert(newEntity, cont);

        _inventory.TransferEntityInventories(ev.Performer, newEntity);
        foreach (var hand in Hands.EnumerateHeld(ev.Performer))
        {
            Hands.TryDrop(ev.Performer, hand, checkActionBlocker: false);
            Hands.TryPickupAnyHand(newEntity, hand);
        }

        var name = meta.EntityName;

        Meta.SetEntityName(newEntity, name);

        int? age = null;
        Gender? gender = null;
        Sex? sex = null;
        if (TryComp(ev.Performer, out HumanoidAppearanceComponent? humanoid))
        {
            age = humanoid.Age;
            gender = humanoid.Gender;
            sex = humanoid.Sex;
            if (TryComp(newEntity, out HumanoidAppearanceComponent? newHumanoid))
            {
                newHumanoid.Age = age.Value;
                newHumanoid.Gender = gender.Value;
                newHumanoid.Sex = sex.Value;
                Dirty(newEntity, newHumanoid);
            }
        }

        SetGear(newEntity, ev.Gear, false, false);

        Mind.TransferTo(mind, newEntity, mind: mindComponent);

        Body.GibBody(ev.Performer, contents: GibContentsOption.Gib);

        Faction.ClearFactions(newEntity, false);
        Faction.AddFaction(newEntity, WizardRuleSystem.Faction);
        RemCompDeferred<TransferMindOnGibComponent>(newEntity);
        EnsureComp<WizardComponent>(newEntity);
        if (!Role.MindHasRole<WizardRoleComponent>(mind, out _))
            Role.MindAddRole(mind, WizardRuleSystem.Role.Id, mindComponent, true);
        EnsureComp<PhylacteryComponent>(item);
        var soulBound = EntityManager.ComponentFactory.GetComponent<SoulBoundComponent>();
        soulBound.Name = name;
        soulBound.Item = item;
        soulBound.MapId = mapId;
        soulBound.Age = age;
        soulBound.Gender = gender;
        soulBound.Sex = sex;
        AddComp(mind, soulBound, true);

        if (ev.Speech != null)
            _chat.TrySendInGameICMessage(newEntity, Loc.GetString(ev.Speech), InGameICChatType.Speak, false);

        if (mindComponent.Session == null)
            return;

        _antag.SendBriefing(mindComponent.Session, Loc.GetString("lich-greeting"), Color.DarkRed, ev.Sound);
    }

    protected override bool Polymorph(PolymorphSpellEvent ev)
    {
        if (ev.ProtoId == null)
            return false;

        var newEnt = _polymorph.PolymorphEntity(ev.Performer, ev.ProtoId.Value);

        if (newEnt == null)
            return false;

        if (ev.MakeWizard && HasComp<WizardComponent>(ev.Performer))
            EnsureComp<WizardComponent>(newEnt.Value);

        if (ev.Speech != null)
            _chat.TrySendInGameICMessage(newEnt.Value, Loc.GetString(ev.Speech), InGameICChatType.Speak, false);

        return true;
    }

    protected override void ShootSpellCards(SpellCardsEvent ev, EntProtoId proto)
    {
        base.ShootSpellCards(ev, proto);

        MapCoordinates targetMap;

        if (ev.Coords != null)
            targetMap = TransformSystem.ToMapCoordinates(ev.Coords.Value);
        else if (TryComp(ev.Entity, out TransformComponent? xform))
            targetMap = TransformSystem.GetMapCoordinates(ev.Entity.Value, xform);
        else
            return;

        var (_, mapCoords, spawnCoords, velocity) = GetProjectileData(ev.Performer);

        var mapDirection = targetMap.Position - mapCoords.Position;
        var mapAngle = mapDirection.ToAngle();

        var angles = _gun.LinearSpread(mapAngle - ev.Spread / 2, mapAngle + ev.Spread / 2, ev.ProjectilesAmount);

        var linearDamping = Random.NextFloat() * (ev.MinMaxLinearDamping.Y - ev.MinMaxLinearDamping.X) +
                            ev.MinMaxLinearDamping.X;

        var setHoming = Exists(ev.Entity) && ev.Entity != ev.Performer && HasComp<MobStateComponent>(ev.Entity);

        for (var i = 0; i < ev.ProjectilesAmount; i++)
        {
            var newUid = Spawn(proto, spawnCoords);
            _gun.ShootProjectile(newUid, angles[i].ToVec(), velocity, ev.Performer, ev.Performer, ev.ProjectileSpeed);

            if (!TryComp(newUid, out PhysicsComponent? physics))
                continue;

            Physics.SetAngularVelocity(newUid,
                (Random.NextFloat() - 0.5f) * ev.MaxAngularVelocity,
                false,
                body: physics);
            Physics.SetLinearDamping(newUid, physics, linearDamping, false);

            var spellCard = EnsureComp<SpellCardComponent>(newUid);
            if (!setHoming)
            {
                Dirty(newUid, physics);
                continue;
            }

            spellCard.Target = ev.Entity;
            _gun.SetTarget(newUid, ev.Entity, out var targeted, false);
            Entity<SpellCardComponent, PhysicsComponent, TargetedProjectileComponent> ent = (newUid, spellCard, physics,
                targeted);
            Dirty(ent);
        }
    }
}
