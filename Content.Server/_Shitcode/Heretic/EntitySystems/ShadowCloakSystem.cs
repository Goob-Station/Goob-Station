using Content.Server.Buckle.Systems;
using Content.Server.IdentityManagement;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared._Shitcode.Heretic.Systems;
using Content.Shared.FixedPoint;
using Content.Shared.Heretic;
using Content.Shared.Interaction;

namespace Content.Server.Heretic.EntitySystems;

public sealed class ShadowCloakSystem : SharedShadowCloakSystem
{
    [Dependency] private readonly IdentitySystem _identity = default!;
    [Dependency] private readonly ProtectiveBladeSystem _blade = default!;

    private const float SustainedDamageReductionInterval = 1f;
    private float _accumulator;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowCloakEntityComponent, InteractHandEvent>(OnInteractHand,
            after: [typeof(BuckleSystem)]);
    }

    private void OnInteractHand(Entity<ShadowCloakEntityComponent> ent, ref InteractHandEvent args)
    {
        if (args.Handled)
            return;

        var parent = Transform(ent).ParentUid;

        if (args.User != parent || !HasComp<HereticComponent>(parent))
            return;

        if (_blade.TryThrowProtectiveBlade(parent, null))
            args.Handled = true;
    }

    protected override void Startup(Entity<ShadowCloakedComponent> ent)
    {
        base.Startup(ent);

        _identity.QueueIdentityUpdate(ent);
    }

    protected override void Shutdown(Entity<ShadowCloakedComponent> ent)
    {
        base.Shutdown(ent);

        _identity.QueueIdentityUpdate(ent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var shadowEntityQuery = AllEntityQuery<ShadowCloakEntityComponent>();
        while (shadowEntityQuery.MoveNext(out var uid, out var comp))
        {
            if (comp.DeletionAccumulator == null)
                continue;

            comp.DeletionAccumulator -= frameTime;

            if (comp.DeletionAccumulator > 0)
                continue;

            QueueDel(uid);
        }

        _accumulator += frameTime;

        if (_accumulator < SustainedDamageReductionInterval)
            return;

        _accumulator = 0f;

        var shadowCloakedQuery = EntityQueryEnumerator<ShadowCloakedComponent>();
        while (shadowCloakedQuery.MoveNext(out _, out var comp))
        {
            comp.SustainedDamage =
                FixedPoint2.Max(comp.SustainedDamage - comp.SustainedDamageReductionRate, FixedPoint2.Zero);
        }
    }
}
