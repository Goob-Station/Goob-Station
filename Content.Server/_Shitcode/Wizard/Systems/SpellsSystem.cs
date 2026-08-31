// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using System.Numerics;
using Content.Goobstation.Common.Actions;
using Content.Goobstation.Common.Bloodstream;
using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Emoting;
using Content.Goobstation.Shared.Teleportation.Systems;
using Content.Goobstation.Shared.Religion;
using Content.Server._Goobstation.Wizard.Components;
using Content.Server.Antag;
using Content.Server.Body.Systems;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.Emp;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Fluids.EntitySystems;
using Content.Shared.IdentityManagement;
using Content.Server.Inventory;
using Content.Server.Polymorph.Systems;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Server.Singularity.EntitySystems;
using Content.Server.Spreader;
using Content.Server.Store.Components;
using Content.Server.Store.Systems;
using Content.Server.Weapons.Ranged.Systems;
using Content.Shared._Goobstation.Wizard;
using Content.Shared._Goobstation.Wizard.BindSoul;
using Content.Shared._Goobstation.Wizard.Chuuni;
using Content.Shared._Shitmed.Damage; // Shitmed Change
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Actions.Components;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Chat;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Construction.Components;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.Friction;
using Content.Shared.Gibbing.Events;
using Content.Shared.Hands.Components;
using Content.Shared.Humanoid;
using Content.Shared.IdentityManagement;
using Content.Shared.Item;
using Content.Shared.Magic.Components;
using Content.Shared.Maps;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.NPC.Systems;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.Random.Helpers;
using Content.Shared.Speech.Components;
using Content.Shared.Tag;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Server.Player;
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
using Content.Shared.Actions.Components;
using Content.Shared.Body.Components;
using Content.Shared.Construction.Components;
using Content.Shared.Friction;
using Content.Shared.Item;
using Content.Shared.Tag;
using Content.Goobstation.Shared.Teleportation.Systems;
using Content.Shared._Shitcode.Wizard.Components;
using Content.Shared.Power.Components;

namespace Content.Server._Goobstation.Wizard.Systems; //todo refactor wiz

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
    [Dependency] private readonly IdentitySystem _identity = default!;
    [Dependency] private readonly BatterySystem _battery = default!;
    [Dependency] private readonly SharedRandomTeleportSystem _teleport = default!;
    [Dependency] private readonly NpcFactionSystem _faction = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly TurfSystem _turf = default!;
    [Dependency] private readonly SharedItemSystem _item = default!;
    [Dependency] private readonly TileFrictionController _tileFriction = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly WoundSystem _wounds = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly PuddleSystem _puddle = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly DivineInterventionSystem _divineIntervention = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MindContainerComponent, SummonSimiansMaxedOutEvent>(OnMonkeyAscension);
    }

    protected override void CreateChargeEffect(EntityUid uid, ChargeSpellRaysEffectEvent ev)
    {
        RaiseNetworkEvent(ev, Filter.PvsExcept(uid));
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

        if (!_player.TryGetSessionById(mindComp.UserId, out var session))
            return;

        var message = Loc.GetString("spell-summon-simians-maxed-out-message");
        var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", message));
        _chatManager.ChatMessageToOne(ChatChannel.Server,
            message,
            wrappedMessage,
            default,
            false,
            session.Channel,
            args.MessageColor);
    }

    protected override void Emote(EntityUid uid, string emoteId)
    {
        base.Emote(uid, emoteId);

        _chat.TryEmoteWithChat(uid, emoteId);
    }

    protected override void BindSoul(BindSoulEvent ev, EntityUid item, EntityUid mind, MindComponent mindComponent)
    {
        base.BindSoul(ev, item, mind, mindComponent);

        var oldEnt = ev.Performer;
        var xform = Transform(oldEnt);
        var meta = MetaData(oldEnt);

        var mapId = xform.MapUid;

        var newEntity = Spawn(ev.Entity,
            TransformSystem.GetMapCoordinates(oldEnt, xform),
            rotation: TransformSystem.GetWorldRotation(oldEnt));

        if (Container.TryGetContainingContainer((oldEnt, xform, meta), out var cont))
            Container.Insert(newEntity, cont);

        var name = meta.EntityName;

        Meta.SetEntityName(newEntity, name);

        int? age = null;
        Gender? gender = null;
        Sex? sex = null;
        if (TryComp(oldEnt, out HumanoidAppearanceComponent? humanoid))
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
        if (!Role.MindHasRole<GoobWizardRoleComponent>(mind, out _))
            Role.MindAddRole(mind, WizardRuleSystem.Role.Id, mindComponent, true);

        EnsureComp<PhylacteryComponent>(item);
        _item.SetSize(item, ev.PhylacterySize);
        RemCompDeferred<TagComponent>(item);
        RemCompDeferred<AnchorableComponent>(item);

        var soulBound = EntityManager.ComponentFactory.GetComponent<SoulBoundComponent>();
        soulBound.Name = name;
        soulBound.Item = item;
        soulBound.MapId = mapId;
        soulBound.Age = age;
        soulBound.Gender = gender;
        soulBound.Sex = sex;
        AddComp(mind, soulBound, true);

        _inventory.TransferEntityInventories(oldEnt, newEntity);
        foreach (var hand in Hands.EnumerateHeld(oldEnt))
        {
            Hands.TryDrop(oldEnt, hand, checkActionBlocker: false);
            Hands.TryPickupAnyHand(newEntity, hand);
        }

        SetGear(newEntity, ev.Gear, false, false);

        if (TryComp(ev.Action.Owner, out SpeakOnActionComponent? speak))
        {
            DelayedSpeech(speak.Sentence == null ? null : Loc.GetString(speak.Sentence.Value),
                newEntity,
                oldEnt,
                MagicSchool.Necromancy);
        }

        Body.GibBody(oldEnt, contents: GibContentsOption.Gib);

        if (!_player.TryGetSessionById(mindComponent.UserId, out var session))
            return;

        _antag.SendBriefing(session, Loc.GetString("lich-greeting"), Color.DarkRed, ev.Sound);
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

        if (ev.LoadActions)
            RaiseNetworkEvent(new LoadActionsEvent(GetNetEntity(ev.Performer)), newEnt.Value);

        if (TryComp(ev.Action.Owner, out SpeakOnActionComponent? speak))
        {
            DelayedSpeech(speak.Sentence == null ? null : Loc.GetString(speak.Sentence.Value),
                newEnt.Value,
                ev.Performer,
                school);
        }

        return true;
    }
    private void DelayedSpeech(string? speech, EntityUid speaker, EntityUid caster, MagicSchool school)
    {
        Timer.Spawn(200,
            () =>
            {
                var toSpeak = speech == null ? string.Empty : Loc.GetString(speech);
                SpeakSpell(speaker, caster, toSpeak, school);
            });
    }

    protected override void Speak(EntityUid uid, string message)
    {
        base.Speak(uid, message);

        _chat.TrySendInGameICMessage(uid, message, InGameICChatType.Speak, false);
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

    public override void SpeakSpell(EntityUid speakerUid, EntityUid casterUid, string speech, MagicSchool school)
    {
        base.SpeakSpell(speakerUid, casterUid, speech, school);

        if (!Exists(speakerUid))
            return;

        Color? color = null;

        if (Exists(casterUid))
        {
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
                    targetPart: TargetBodyPart.All,
                    splitDamage: SplitDamageBehavior.SplitEnsureAll);

                if (speakerUid != casterUid)
                {
                    Damageable.TryChangeDamage(speakerUid,
                        -invocationEv.ToHeal,
                        true,
                        false,
                        targetPart: TargetBodyPart.All,
                        splitDamage: SplitDamageBehavior.SplitEnsureAll);
                }
            }

            if (speakerUid != casterUid)
            {
                var colorEv = new GetMessageColorOverrideEvent();
                RaiseLocalEvent(casterUid, colorEv);
                color = colorEv.Color;
            }
        }

        _chat.TrySendInGameICMessage(speakerUid,
            speech,
            InGameICChatType.Speak,
            false,
            colorOverride: color);
    }

    protected override bool ChargeItem(EntityUid uid, ChargeMagicEvent ev)
    {
        if (!TryComp(uid, out BatteryComponent? battery) || battery.LastCharge >= battery.MaxCharge)
            return false;

        if (Tag.HasTag(uid, ev.WandTag))
        {
            var difference = battery.MaxCharge - battery.LastCharge;
            var charge = MathF.Min(difference, ev.WandChargeRate);
            var degrade = charge * ev.WandDegradePercentagePerCharge;
            var afterDegrade = MathF.Max(ev.MinWandDegradeCharge, battery.MaxCharge - degrade);
            if (battery.MaxCharge > ev.MinWandDegradeCharge)
                _battery.SetMaxCharge(uid, afterDegrade);
            _battery.SetCharge(uid, battery.LastCharge + charge);
        }
        else
            _battery.SetCharge(uid, battery.MaxCharge);

        PopupCharged(uid, ev.Performer, false);
        return true;
    }
}
