// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using System.Numerics;
using Content.Goobstation.Common.Actions;
using Content.Goobstation.Common.Bloodstream;
using Content.Server._Goobstation.Wizard.Components;
using Content.Server.Abilities.Mime;
using Content.Server.Antag;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.Emp;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Fluids.EntitySystems;
using Content.Server.IdentityManagement;
using Content.Server.Inventory;
using Content.Server.Polymorph.Systems;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Server.Singularity.EntitySystems;
using Content.Server.Spreader;
using Content.Server.Store.Components;
using Content.Server.Store.Systems;
using Content.Server.Teleportation;
using Content.Server.Weapons.Ranged.Systems;
using Content.Shared._Goobstation.Wizard;
using Content.Shared._Goobstation.Wizard.BindSoul;
using Content.Shared._Goobstation.Wizard.Chuuni;
using Content.Shared._Goobstation.Wizard.FadingTimedDespawn;
using Content.Shared._Goobstation.Wizard.SpellCards;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Actions;
using Content.Shared.Chat;
using Content.Shared.Chemistry.Components;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.FixedPoint;
using Content.Shared.Gibbing.Events;
using Content.Shared.Hands.Components;
using Content.Shared.Humanoid;
using Content.Shared.IdentityManagement;
using Content.Shared.Magic.Components;
using Content.Shared.Maps;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.NPC.Systems;
using Content.Shared.Physics;
using Content.Shared.Random.Helpers;
using Content.Shared.Speech.Components;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.Enums;
using Robust.Shared.GameObjects.Components.Localization;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Spawners;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server._Goobstation.Wizard.Systems;

public sealed class SpellsSystem : SharedSpellsSystem
{
    [Dependency] private readonly IChatManager _chatManager = default!;
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
    [Dependency] private readonly BloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly IdentitySystem _identity = default!;
    [Dependency] private readonly BatterySystem _battery = default!;
    [Dependency] private readonly TeleportSystem _teleport = default!;
    [Dependency] private readonly NpcFactionSystem _faction = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MindContainerComponent, SummonSimiansMaxedOutEvent>(OnMonkeyAscension);
        SubscribeLocalEvent<BloodlossDamageMultiplierComponent, StoppedTakingBloodlossDamageEvent>(OnBloodlossStopped);
        SubscribeLocalEvent<BloodlossDamageMultiplierComponent, GetBloodlossDamageMultiplierEvent>(OnGetBloodlossMultiplier);
    }

    private void OnGetBloodlossMultiplier(Entity<BloodlossDamageMultiplierComponent> ent,
        ref GetBloodlossDamageMultiplierEvent args)
    {
        args.Multiplier *= ent.Comp.Multiplier;
    }

    protected override void CreateChargeEffect(EntityUid uid, ChargeSpellRaysEffectEvent ev)
    {
        RaiseNetworkEvent(ev, Filter.PvsExcept(uid));
    }

    private void OnBloodlossStopped(Entity<BloodlossDamageMultiplierComponent> ent,
        ref StoppedTakingBloodlossDamageEvent args)
    {
        RemCompDeferred(ent.Owner, ent.Comp);
    }

    private void OnMonkeyAscension(Entity<MindContainerComponent> ent, ref SummonSimiansMaxedOutEvent args)
    {
        var (uid, comp) = ent;
        if (!TryComp(comp.Mind, out MindComponent? mindComp) ||
            !TryComp(comp.Mind.Value, out ActionsContainerComponent? container))
            return;

        var hasMaxLevelSimians = false;
        var hasGorillaForm = false;
        foreach (var (action, _) in Actions.GetActions(uid))
        {
            if (!hasGorillaForm && Tag.HasTag(action, args.GorillaFormTag))
                hasGorillaForm = true;

            if (!Tag.HasTag(action, args.MaxLevelTag))
                continue;

            if (TryComp(action, out StoreRefundComponent? refund))
                StoreSystem.DisableListingRefund(refund.Data);

            hasMaxLevelSimians = true;
        }

        if (hasGorillaForm || !hasMaxLevelSimians)
            return;

        ActionContainer.AddAction(comp.Mind.Value, args.Action, container);

        if (mindComp.Session == null)
            return;

        var message = Loc.GetString("spell-summon-simians-maxed-out-message");
        var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", message));
        _chatManager.ChatMessageToOne(ChatChannel.Server,
            message,
            wrappedMessage,
            default,
            false,
            mindComp.Session.Channel,
            args.MessageColor);
    }

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

        Spawn(ev.Effect, coords);
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

        _smoke.StartSmoke(ent, new Solution("ThickSmoke", 50), ev.Duration, ev.SpreadAmount, smoke);
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

            Spawn(ev.EffectProto, TransformSystem.GetMapCoordinates(entity, xform));

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
                if (TryComp(newEntity, out GrammarComponent? grammar))
                    Grammar.SetGender((newEntity, grammar), gender.Value);
                var identity = Identity.Entity(newEntity, EntityManager);
                if (TryComp(identity, out GrammarComponent? identityGrammar))
                    Grammar.SetGender((identity, identityGrammar), gender.Value);
            }
        }

        _identity.QueueIdentityUpdate(newEntity);

        Mind.TransferTo(mind, newEntity, mind: mindComponent);

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

        DelayedSpeech(ev.Speech, newEntity, ev.Performer, MagicSchool.Necromancy);

        _inventory.TransferEntityInventories(ev.Performer, newEntity);
        foreach (var hand in Hands.EnumerateHeld(ev.Performer))
        {
            Hands.TryDrop(ev.Performer, hand, checkActionBlocker: false);
            Hands.TryPickupAnyHand(newEntity, hand);
        }

        SetGear(newEntity, ev.Gear, false, false);

        Body.GibBody(ev.Performer, contents: GibContentsOption.Gib);

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

        if (ev.MakeWizard)
        {
            if (HasComp<WizardComponent>(ev.Performer))
                EnsureComp<WizardComponent>(newEnt.Value);
            if (HasComp<ApprenticeComponent>(ev.Performer))
                EnsureComp<ApprenticeComponent>(newEnt.Value);
        }

        Audio.PlayPvs(ev.Sound, newEnt.Value);

        var school = MagicSchool.Transmutation;
        if (TryComp(ev.Action.Owner, out MagicComponent? magic))
            school = magic.School;

        DelayedSpeech(ev.Speech, newEnt.Value, ev.Performer, school);

        if (ev.LoadActions)
            RaiseNetworkEvent(new LoadActionsEvent(GetNetEntity(ev.Performer)), newEnt.Value);

        return true;
    }

    private void DelayedSpeech(string? speech, EntityUid speaker, EntityUid caster, MagicSchool school)
    {
        Timer.Spawn(200,
            () =>
            {
                if (!Exists(speaker) || !Exists(caster))
                    return;

                var toSpeak = speech == null ? string.Empty : Loc.GetString(speech);
                SpeakSpell(speaker, caster, toSpeak, school);
            });
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
        if (mapDirection == Vector2.Zero)
            return;
        var mapAngle = mapDirection.ToAngle();

        var angles = _gun.LinearSpread(mapAngle - ev.Spread / 2, mapAngle + ev.Spread / 2, ev.ProjectilesAmount);

        var linearDamping = Random.NextFloat(ev.MinMaxLinearDamping.X, ev.MinMaxLinearDamping.Y);

        var setHoming = Exists(ev.Entity) && ev.Entity != ev.Performer && HasComp<MobStateComponent>(ev.Entity);

        for (var i = 0; i < ev.ProjectilesAmount; i++)
        {
            var newUid = Spawn(proto, spawnCoords);
            _gun.ShootProjectile(newUid, angles[i].ToVec(), velocity, ev.Performer, ev.Performer, ev.ProjectileSpeed);

            if (!TryComp(newUid, out PhysicsComponent? physics))
                continue;

            Physics.SetAngularVelocity(newUid,
                Random.NextFloat(-ev.MaxAngularVelocity, ev.MaxAngularVelocity),
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

    protected override void Speak(EntityUid uid, string message)
    {
        base.Speak(uid, message);

        _chat.TrySendInGameICMessage(uid, message, InGameICChatType.Speak, false);
    }

    protected override bool ScreamForMe(ScreamForMeEvent ev)
    {
        if (!TryComp(ev.Target, out BloodstreamComponent? bloodstream))
            return false;

        if (TryComp(ev.Target, out VocalComponent? vocal))
            Emote(ev.Target, vocal.ScreamId);

        Spawn(ev.Effect, TransformSystem.GetMapCoordinates(ev.Target));

        _bloodstream.SpillAllSolutions(ev.Target, bloodstream);
        _bloodstream.TryModifyBleedAmount(ev.Target, bloodstream.MaxBleedAmount, bloodstream);
        EnsureComp<BloodlossDamageMultiplierComponent>(ev.Target);

        return true;
    }

    private IEnumerable<MapCoordinates> GetSpawnCoordinatesAroundPerformer(EntityUid performer,
        float range,
        int amount,
        Angle angle,
        int collisionMask)
    {
        var xform = Transform(performer);
        var (pos, rot) = TransformSystem.GetWorldPositionRotation(xform);

        var positions = _gun.LinearSpread(rot - angle, rot + angle, amount)
            .Select(x => new MapCoordinates(pos + x.ToWorldVec() * range, xform.MapID));

        foreach (var position in positions)
        {
            var dir = (position.Position - pos).Normalized();

            var ray = new CollisionRay(pos, dir, collisionMask);

            var result = Physics.IntersectRay(xform.MapID, ray, range, performer).FirstOrNull();

            if (result != null)
                yield return new MapCoordinates(result.Value.HitPos, xform.MapID);
            else
                yield return position;
        }
    }

    protected override void SpawnMobs(SummonMobsEvent ev)
    {
        base.SpawnMobs(ev);

        if (ev.Mobs.Count == 0)
            return;

        var positions =
            GetSpawnCoordinatesAroundPerformer(ev.Performer, ev.Range, ev.Amount, ev.SpawnAngle, ev.CollisionMask);
        foreach (var pos in positions)
        {
            var mob = Spawn(Random.Pick(ev.Mobs), pos);

            if (ev.FactionIgnoreSummoner)
                _faction.IgnoreEntity(mob, ev.Performer);
        }
    }

    protected override void SpawnMonkeys(SummonSimiansEvent ev)
    {
        base.SpawnMonkeys(ev);

        if (!ProtoMan.TryIndex(ev.Mobs, out var mobs) || !ProtoMan.TryIndex(ev.Weapons, out var weapons))
            return;

        if (mobs.Weights.Count == 0)
            return;

        var handsQuery = GetEntityQuery<HandsComponent>();
        var despawnQuery = GetEntityQuery<TimedDespawnComponent>();
        var fadingQuery = GetEntityQuery<FadingTimedDespawnComponent>();

        var positions = GetSpawnCoordinatesAroundPerformer(ev.Performer,
            ev.Range,
            ev.Amount,
            ev.SpawnAngle,
            (int) CollisionGroup.MobMask);
        foreach (var pos in positions)
        {
            var mob = Spawn(mobs.Pick(Random), pos);

            if (!handsQuery.TryComp(mob, out var hands) || hands.Count == 0 || weapons.Weights.Count == 0)
                continue;

            var weapon = Spawn(weapons.Pick(Random), pos);

            if (!Hands.TryPickupAnyHand(mob, weapon, true, false, false, hands))
            {
                QueueDel(weapon);
                continue;
            }

            FadingTimedDespawnComponent? weaponDespawn;
            if (despawnQuery.TryComp(mob, out var despawn))
            {
                weaponDespawn = EnsureComp<FadingTimedDespawnComponent>(weapon);
                weaponDespawn.Lifetime = despawn.Lifetime + 30f;
                weaponDespawn.FadeOutTime = 4f;
                Dirty(weapon, weaponDespawn);
            }
            else if (fadingQuery.TryComp(mob, out var fading))
            {
                weaponDespawn = EnsureComp<FadingTimedDespawnComponent>(weapon);
                weaponDespawn.Lifetime = fading.Lifetime + 30f;
                weaponDespawn.FadeOutTime = 4f;
                Dirty(weapon, weaponDespawn);
            }
        }
    }

    public override void SpeakSpell(EntityUid speakerUid, EntityUid casterUid, string speech, MagicSchool school)
    {
        base.SpeakSpell(speakerUid, casterUid, speech, school);

        var postfix = string.Empty;

        var invocationEv = new GetSpellInvocationEvent(school, casterUid);
        RaiseLocalEvent(casterUid, invocationEv);
        if (invocationEv.Invocation != null)
            speech = Loc.GetString(invocationEv.Invocation);
        if (invocationEv.ToHeal.GetTotal() > FixedPoint2.Zero)
        {
            // Heal both caster and speaker
            Damageable.TryChangeDamage(casterUid,
                -invocationEv.ToHeal,
                true,
                false,
                canSever: false,
                targetPart: TargetBodyPart.All);

            if (speakerUid != casterUid)
            {
                Damageable.TryChangeDamage(speakerUid,
                    -invocationEv.ToHeal,
                    true,
                    false,
                    canSever: false,
                    targetPart: TargetBodyPart.All);
            }
        }

        if (speakerUid != casterUid)
        {
            var postfixEv = new GetMessagePostfixEvent();
            RaiseLocalEvent(casterUid, postfixEv);
            postfix = postfixEv.Postfix;
        }

        _chat.TrySendInGameICMessage(speakerUid,
            speech,
            InGameICChatType.Speak,
            false,
            wrappedMessagePostfix: postfix);
    }

    protected override bool ChargeItem(EntityUid uid, ChargeMagicEvent ev)
    {
        if (!TryComp(uid, out BatteryComponent? battery) || battery.CurrentCharge >= battery.MaxCharge)
            return false;

        if (Tag.HasTag(uid, ev.WandTag))
        {
            var difference = battery.MaxCharge - battery.CurrentCharge;
            var charge = MathF.Min(difference, ev.WandChargeRate);
            var degrade = charge * ev.WandDegradePercentagePerCharge;
            var afterDegrade = MathF.Max(ev.MinWandDegradeCharge, battery.MaxCharge - degrade);
            if (battery.MaxCharge > ev.MinWandDegradeCharge)
                _battery.SetMaxCharge(uid, afterDegrade, battery);
            _battery.AddCharge(uid, charge, battery);
        }
        else
            _battery.SetCharge(uid, battery.MaxCharge, battery);

        PopupCharged(uid, ev.Performer, false);
        return true;
    }

    protected override void Blink(BlinkSpellEvent ev)
    {
        base.Blink(ev);

        _teleport.RandomTeleport(ev.Performer, ev.Radius);
    }
}
