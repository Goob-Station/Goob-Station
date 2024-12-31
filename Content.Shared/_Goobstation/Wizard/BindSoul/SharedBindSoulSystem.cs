using System.Diagnostics.CodeAnalysis;
using Content.Shared._Goobstation.Wizard.Projectiles;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Examine;
using Content.Shared.Ghost;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs;
using Content.Shared.Roles;
using Content.Shared.Stunnable;
using Content.Shared.Tag;
using Robust.Shared.Network;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Spawners;
using Robust.Shared.Utility;

namespace Content.Shared._Goobstation.Wizard.BindSoul;

public abstract class SharedBindSoulSystem : EntitySystem
{
    [Dependency] protected readonly SharedTransformSystem TransformSystem = default!;
    [Dependency] protected readonly SharedMindSystem Mind = default!;
    [Dependency] protected readonly SharedStunSystem Stun = default!;
    [Dependency] protected readonly MetaDataSystem Meta = default!;
    [Dependency] private   readonly TagSystem _tag = default!;
    [Dependency] private   readonly SharedActionsSystem _actions = default!;
    [Dependency] private   readonly DamageableSystem _damageable = default!;
    [Dependency] private   readonly SharedPhysicsSystem _physics = default!;
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
            !TryComp(mind, out SoulBoundComponent? soulBound) || !ItemExistsAndOnSamePlane(soulBound.Item, mapUid))
            return;

        Mind.TransferTo(mind, null, mind: mindComponent);
    }

    private void OnMindGetRemoved(Entity<SoulBoundComponent> ent, ref MindGotRemovedEvent args)
    {
        if (_net.IsClient || _tag.HasTag(args.Container, IgnoreBindSoulTag) || HasComp<GhostComponent>(args.Container))
            return;

        var xform = Transform(args.Container);

        ent.Comp.MapId = xform.MapUid;
        Dirty(ent);

        var coords = TransformSystem.GetMapCoordinates(args.Container, xform);

        _damageable.TryChangeDamage(args.Container,
            new DamageSpecifier(_proto.Index<DamageTypePrototype>("Blunt"), 1000),
            true,
            false,
            targetPart: TargetBodyPart.Torso);

        if (!TerminatingOrDeleted(args.Container) && !EntityManager.IsQueuedForDeletion(args.Container))
            QueueDel(args.Container);

        var item = ent.Comp.Item;

        if (!ItemExistsAndOnSamePlane(item, xform.MapUid))
            return;

        var itemCoords = TransformSystem.GetMapCoordinates(item.Value);
        var particle = Spawn(ParticlePrototype, coords);
        var direction = itemCoords.Position - coords.Position;
        _physics.SetLinearVelocity(particle, direction.Normalized());
        EnsureComp<TimedDespawnComponent>(particle).Lifetime = 5f * (1 + ent.Comp.ResurrectionsCount);
        var homing = EnsureComp<HomingProjectileComponent>(particle);
        homing.Target = item.Value;
        Dirty(particle, homing);
    }

    private bool ItemExistsAndOnSamePlane([NotNullWhen(true)] EntityUid? item, EntityUid? mapUid)
    {
        return TryComp(item, out TransformComponent? xform) && xform.MapUid != null && xform.MapUid == mapUid;
    }

    private void OnMindGetAdded(Entity<SoulBoundComponent> ent, ref MindGotAddedEvent args)
    {
        var (uid, comp) = ent;

        if (!HasComp<GhostComponent>(args.Container))
            return;

        if (!TryComp(uid, out ActionsContainerComponent? container))
            return;

        var action = container.Container.ContainedEntities.FirstOrNull(x => _tag.HasTag(x, ActionTag));

        if (action == null)
            return;

        _actions.SetUseDelay(action.Value,
            TimeSpan.FromMinutes(3) + TimeSpan.FromSeconds(30) * comp.ResurrectionsCount);
        _actions.StartUseDelay(action.Value);
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

    protected virtual void MakeDestructible(EntityUid uid)
    {
    }
}
