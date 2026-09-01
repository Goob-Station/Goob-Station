using System.Linq;
using Content.Goobstation.CommonShared.Wizard.Components;
using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Religion;
using Content.Goobstation.Shared.Wizard.Components;
using Content.Goobstation.Shared.Wizard.Systems.Spells;
using Content.Server.Actions;
using Content.Server.Antag;
using Content.Server.Body.Systems;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.Fluids.EntitySystems;
using Content.Server.Hands.Systems;
using Content.Server.Inventory;
using Content.Server.Polymorph.Systems;
using Content.Server.Spreader;
using Content.Server.Store.Systems;
using Content.Server.Weapons.Ranged.Systems;
using Content.Shared._Goobstation.Wizard.Chuuni;
using Content.Shared._Shitmed.Damage;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Actions;
using Content.Shared.Chat;
using Content.Shared.Damage;
using Content.Shared.Friction;
using Content.Shared.Hands.Components;
using Content.Shared.Inventory;
using Content.Shared.Magic.Components;
using Content.Shared.Maps;
using Content.Shared.Mobs.Systems;
using Content.Shared.NPC.Systems;
using Content.Shared.StatusEffect;
using Content.Shared.Tag;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Server.Player;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Spawners;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.Wizard.Systems;

public sealed partial class SpellsSystem : SharedSpellsSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly DivineInterventionSystem _divineIntervention = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly WoundSystem _wound = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;

    [Dependency] private readonly TransformSystem _xform = default!;
    [Dependency] private readonly MapSystem _map = default!;
    [Dependency] private readonly TurfSystem _turf = default!;
    [Dependency] private readonly SpreaderSystem _spreader = default!;
    [Dependency] private readonly SmokeSystem _smoke = default!;
    [Dependency] private readonly GunSystem _gun = default!;
    [Dependency] private readonly PhysicsSystem _physics = default!;
    [Dependency] private readonly TileFrictionController _tileFrictionController = default!;
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;
    [Dependency] private readonly StoreSystem _store = default!;
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly NpcFactionSystem _faction = default!;
    [Dependency] private readonly ServerInventorySystem _serverInventory = default!;
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;

    private EntityQuery<HandsComponent> _handsQuery;
    private EntityQuery<TimedDespawnComponent> _timedDespawnQuery;
    private EntityQuery<FadingTimedDespawnComponent> _fadingTimedDespawnQuery;
    private EntityQuery<PhysicsComponent> _physicsQuery;

    public override void Initialize()
    {
        base.Initialize();

        _handsQuery = GetEntityQuery<HandsComponent>();
        _timedDespawnQuery = GetEntityQuery<TimedDespawnComponent>();
        _fadingTimedDespawnQuery = GetEntityQuery<FadingTimedDespawnComponent>();
        _physicsQuery = GetEntityQuery<PhysicsComponent>();
    }

    private IEnumerable<MapCoordinates> GetSpawnCoordinatesAroundPerformer(EntityUid performer,
        float range,
        int amount,
        Angle angle,
        int collisionMask)
    {
        var xform = Transform(performer);
        var (pos, rot) = _xform.GetWorldPositionRotation(xform);

        var positions = _gun.LinearSpread(rot - angle, rot + angle, amount)
            .Select(x => new MapCoordinates(pos + x.ToWorldVec() * range, xform.MapID));

        foreach (var position in positions)
        {
            var dir = (position.Position - pos).Normalized();

            var ray = new CollisionRay(pos, dir, collisionMask);

            var result = _physics.IntersectRay(xform.MapID, ray, range, performer).FirstOrNull();

            if (result != null)
                yield return new MapCoordinates(result.Value.HitPos, xform.MapID);
            else
                yield return position;
        }
    }

    private void SpeakSpell(EntityUid speakerUid, EntityUid casterUid, string speech, MagicSchool school)
    {
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
                _damageable.TryChangeDamage(casterUid,
                    -invocationEv.ToHeal,
                    true,
                    false,
                    targetPart: TargetBodyPart.All,
                    splitDamage: SplitDamageBehavior.SplitEnsureAll);

                if (speakerUid != casterUid)
                {
                    _damageable.TryChangeDamage(speakerUid,
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

    private void DelayedSpeech(string? speech, EntityUid speaker, EntityUid caster, MagicSchool school)
    {
        Timer.Spawn(200,
            () =>
            {
                var toSpeak = speech == null ? string.Empty : Loc.GetString(speech);
                SpeakSpell(speaker, caster, toSpeak, school);
            });
    }
}