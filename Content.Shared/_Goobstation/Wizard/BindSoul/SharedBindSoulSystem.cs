using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Shared._Goobstation.Wizard.Projectiles;
using Content.Shared.Actions;
using Content.Shared.Body.Systems;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.Ghost;
using Content.Shared.Gibbing.Events;
using Content.Shared.Gravity;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs;
using Content.Shared.NPC.Systems;
using Content.Shared.Roles;
using Content.Shared.Stunnable;
using Content.Shared.Tag;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Spawners;

namespace Content.Shared._Goobstation.Wizard.BindSoul;

public abstract class SharedBindSoulSystem : EntitySystem
{
    [Dependency] protected readonly SharedTransformSystem TransformSystem = default!;
    [Dependency] protected readonly SharedMindSystem Mind = default!;
    [Dependency] protected readonly SharedStunSystem Stun = default!;
    [Dependency] protected readonly MetaDataSystem Meta = default!;
    [Dependency] protected readonly SharedContainerSystem Container = default!;
    [Dependency] protected readonly NpcFactionSystem Faction = default!;
    [Dependency] protected readonly GrammarSystem Grammar = default!;
    [Dependency] private   readonly TagSystem _tag = default!;
    [Dependency] private   readonly SharedActionsSystem _actions = default!;
    [Dependency] private   readonly SharedBodySystem _body = default!;
    [Dependency] private   readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private   readonly SharedGravitySystem _gravity = default!;
    [Dependency] private   readonly IPrototypeManager _proto = default!;
    [Dependency] private   readonly INetManager _net = default!;

    public static readonly ProtoId<TagPrototype> IgnoreBindSoulTag = "IgnoreBindSoul"; // Goobstation

    private static readonly ProtoId<TagPrototype> ActionTag = "BindSoulAction";

    private static readonly EntProtoId ParticlePrototype = "BindSoulParticle";

    protected static readonly EntProtoId LichPrototype = "MobSkeletonPerson";

    protected static readonly ProtoId<StartingGearPrototype> LichGear = "LichGear";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PhylacteryComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<PhylacteryComponent, ExaminedEvent>(OnExamined);

        SubscribeLocalEvent<MobStateChangedEvent>(OnMobStateChanged);

        SubscribeLocalEvent<SoulBoundComponent, MindGotAddedEvent>(OnMindGetAdded);
        SubscribeLocalEvent<SoulBoundComponent, MindGotRemovedEvent>(OnMindGetRemoved);
    }

    private void OnMobStateChanged(MobStateChangedEvent ev)
    {
        if (ev.NewMobState != MobState.Dead)
            return;

        var mapUid = Transform(ev.Target).MapUid;

        if (!Mind.TryGetMind(ev.Target, out var mind, out var mindComponent) ||
            !TryComp(mind, out SoulBoundComponent? soulBound) ||
            !ItemExistsAndOnSamePlane(soulBound.Item, mapUid, out _))
            return;

        Mind.TransferTo(mind, null, mind: mindComponent);
    }

    private void OnMindGetRemoved(Entity<SoulBoundComponent> ent, ref MindGotRemovedEvent args)
    {
        if (_net.IsClient || _tag.HasTag(args.Container, IgnoreBindSoulTag) || HasComp<GhostComponent>(args.Container) ||
            Terminating(args.Container))
            return;

        var xform = Transform(args.Container);

        ent.Comp.MapId = xform.MapUid;
        Dirty(ent);

        var coords = TransformSystem.GetMapCoordinates(args.Container, xform);

        if (!Deleting(args.Container))
            _body.GibBody(args.Container, true, contents: GibContentsOption.Skip);

        if (!Deleting(args.Container))
            QueueDel(args.Container);

        var item = ent.Comp.Item;

        if (!ItemExistsAndOnSamePlane(item, xform.MapUid, out var itemXform))
        {
            if (item == null || itemXform == null)
                return;

            // Item exists but on another plane, respawn it
            if (!RespawnItem(item.Value, itemXform, xform))
                return;
        }
        else if ((itemXform.GridUid == null &&
                 (!TryComp(item.Value, out PhysicsComponent? body) ||
                  _gravity.IsWeightless(item.Value, body, itemXform)) ||
                 itemXform.GridUid != xform.GridUid) && // If it is in space or on another grid
                 !RespawnItem(item.Value, itemXform, xform))
            return;

        // If it is somehow on another plane after respawning
        if (xform.MapUid == null || xform.MapUid != itemXform.MapUid)
            return;

        var itemCoords = TransformSystem.GetMapCoordinates(item.Value, itemXform);
        var particle = Spawn(ParticlePrototype, coords);
        var direction = itemCoords.Position - coords.Position;
        _physics.SetLinearVelocity(particle, direction.Normalized());
        EnsureComp<TimedDespawnComponent>(particle).Lifetime = 5f * (1 + ent.Comp.ResurrectionsCount);
        var homing = EnsureComp<HomingProjectileComponent>(particle);
        homing.Target = item.Value;
        Dirty(particle, homing);
    }

    private bool Deleting(EntityUid uid)
    {
        return TerminatingOrDeleted(uid) || EntityManager.IsQueuedForDeletion(uid);
    }

    private bool ItemExistsAndOnSamePlane([NotNullWhen(true)] EntityUid? item,
        EntityUid? mapUid,
        [NotNullWhen(true)] out TransformComponent? xform)
    {
        xform = null;
        return TryComp(item, out xform) && xform.MapUid != null && xform.MapUid == mapUid;
    }

    private void OnMindGetAdded(Entity<SoulBoundComponent> ent, ref MindGotAddedEvent args)
    {
        var (uid, comp) = ent;

        if (!HasComp<GhostComponent>(args.Container))
            return;

        if (!TryComp(uid, out ActionsContainerComponent? container))
            return;

        var delay = TimeSpan.FromMinutes(1) + TimeSpan.FromSeconds(30) * comp.ResurrectionsCount;

        var actions = container.Container.ContainedEntities.Where(x => _tag.HasTag(x, ActionTag));
        foreach (var action in actions)
        {
            _actions.SetUseDelay(action, delay);
            _actions.StartUseDelay(action);
        }
    }

    private void OnExamined(Entity<PhylacteryComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("ensouled-item-desc"));
    }

    private void OnInit(Entity<PhylacteryComponent> ent, ref ComponentInit args)
    {
        Meta.SetEntityName(ent, Loc.GetString("ensouled-item-name", ("item", ent)));

        EnsureComp<DamageableComponent>(ent);

        MakeDestructible(ent);
    }

    public virtual void Resurrect(EntityUid mind,
        EntityUid phylactery,
        MindComponent mindComp,
        SoulBoundComponent soulBound)
    {
    }

    protected virtual bool RespawnItem(EntityUid item, TransformComponent itemXform, TransformComponent userXform)
    {
        return false;
    }

    protected virtual void MakeDestructible(EntityUid uid)
    {
    }
}
