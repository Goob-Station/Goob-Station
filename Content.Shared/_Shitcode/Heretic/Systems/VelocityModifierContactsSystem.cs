using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared.Whitelist;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;

namespace Content.Shared._Shitcode.Heretic.Systems;

public sealed class VelocityModifierContactsSystem : EntitySystem
{
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelistSystem = default!;

    private readonly HashSet<EntityUid> _toUpdate = new();
    private readonly HashSet<EntityUid> _toRemove = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<VelocityModifierContactsComponent, StartCollideEvent>(OnEntityEnter);
        SubscribeLocalEvent<VelocityModifierContactsComponent, EndCollideEvent>(OnEntityExit);
        SubscribeLocalEvent<VelocityModifierContactsComponent, ComponentShutdown>(OnShutdown);

        UpdatesAfter.Add(typeof(SharedPhysicsSystem));
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _toRemove.Clear();

        foreach (var ent in _toUpdate)
        {
            RefreshVelocity(ent);
        }

        foreach (var ent in _toRemove)
        {
            RemComp<VelocityModifiedByContactComponent>(ent);
        }

        _toUpdate.Clear();
    }

    private void OnShutdown(EntityUid uid, VelocityModifierContactsComponent component, ComponentShutdown args)
    {
        if (!TryComp(uid, out PhysicsComponent? phys))
            return;

        _toUpdate.UnionWith(_physics.GetContactingEntities(uid, phys));
    }

    private void RefreshVelocity(EntityUid uid)
    {
        if (!TryComp<PhysicsComponent>(uid, out var physicsComponent) ||
            !TryComp(uid, out VelocityModifiedByContactComponent? modified) || modified.OriginalVelocity == null)
            return;

        var velocity = 0.0f;

        var entries = 0;
        foreach (var ent in _physics.GetContactingEntities(uid, physicsComponent))
        {
            var velocityModified = false;

            if (TryComp<VelocityModifierContactsComponent>(ent, out var slowContactsComponent) &&
                slowContactsComponent.IsActive)
            {
                if (!CheckWhitelist(uid, slowContactsComponent))
                    continue;

                velocity += slowContactsComponent.Modifier;
                velocityModified = true;
            }

            if (!velocityModified)
                continue;

            entries++;
        }

        switch (entries)
        {
            case > 0 when !MathHelper.CloseTo(velocity, entries):
                velocity /= entries;
                _physics.SetLinearVelocity(uid, modified.OriginalVelocity.Value * velocity, body: physicsComponent);
                break;
            case 0:
                _toRemove.Add(uid);
                _physics.SetLinearVelocity(uid, modified.OriginalVelocity.Value, body: physicsComponent);
                break;
        }
    }

    private void OnEntityExit(EntityUid uid, VelocityModifierContactsComponent component, ref EndCollideEvent args)
    {
        if (!component.IsActive)
            return;

        var otherUid = args.OtherEntity;

        if (!CheckWhitelist(otherUid, component))
            return;

        _toUpdate.Add(otherUid);
    }

    private void OnEntityEnter(EntityUid uid, VelocityModifierContactsComponent component, ref StartCollideEvent args)
    {
        if (!component.IsActive)
            return;

        if (!CheckWhitelist(args.OtherEntity, component))
            return;

        AddModifiedEntity(args.OtherEntity);
    }

    /// <summary>
    /// Add an entity to be checked for Velocity modification from contact with another entity.
    /// </summary>
    /// <param name="uid">The entity to be added.</param>
    public void AddModifiedEntity(EntityUid uid)
    {
        if (!TryComp(uid, out PhysicsComponent? physics))
            return;

        var modified = EnsureComp<VelocityModifiedByContactComponent>(uid);
        if (modified.OriginalVelocity == null)
        {
            modified.OriginalVelocity = physics.LinearVelocity;
            Dirty(uid, modified);
        }
        _toUpdate.Add(uid);
    }

    private bool CheckWhitelist(EntityUid uid, VelocityModifierContactsComponent slowContactsComponent)
    {
        return _whitelistSystem.CheckBoth(uid, slowContactsComponent.Blacklist, slowContactsComponent.Whitelist);
    }
}
