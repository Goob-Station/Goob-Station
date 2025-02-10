using Content.Shared._Goobstation.MULE.Components;
using Content.Shared.Buckle.Components;
using Content.Shared.Interaction;
using Robust.Shared.Physics.Systems;

namespace Content.Shared._Goobstation.MULE;

/// <summary>
/// This handles MULE interactions
/// </summary>
public sealed class MuleSystem : EntitySystem
{
    [Dependency] public required SharedPhysicsSystem _physicsSystem = default!;
    [Dependency] public required EntityManager _entityManager = default!;
    [Dependency] public required SharedInteractionSystem _interactionSystem = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<MuleComponent,StrappedEvent>(OnStrap);
        SubscribeLocalEvent<MuleComponent,UnstrappedEvent>(OnUnstrap);
    }

    private void OnUnstrap(EntityUid uid, MuleComponent component, ref UnstrappedEvent args)
    {
        _physicsSystem.SetCanCollide(args.Buckle.Owner, true);
    }

    // prevent the mule from fucking flying
    private void OnStrap(EntityUid uid, MuleComponent component, ref StrappedEvent args)
    {
        _physicsSystem.SetCanCollide(args.Buckle.Owner, false);
    }
}
