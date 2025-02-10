using Content.Shared._Goobstation.MULE.Components;
using Content.Shared.Buckle;
using Content.Shared.Buckle.Components;
using Content.Shared.Interaction;
using Content.Shared.Tag;
using Robust.Shared.Physics.Systems;

namespace Content.Shared._Goobstation.MULE;

/// <summary>
/// This handles the MULE.
/// </summary>
public sealed class MuleSystem : EntitySystem
{
    [Dependency] public required SharedPhysicsSystem _physicsSystem = default!;
    [Dependency] public required EntityManager _entityManager = default!;
    [Dependency] public required SharedInteractionSystem _interactionSystem = default!;
    [Dependency] public required TagSystem _tagSystem = default!;
    [Dependency] public required SharedBuckleSystem _buckleSystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MuleComponent,StrappedEvent>(OnStrap);
        SubscribeLocalEvent<MuleComponent,UnstrappedEvent>(OnUnstrap);
        SubscribeLocalEvent<MuleComponent,StrapAttemptEvent>(OnStrapAttempt);
        SubscribeLocalEvent<MuleComponent, InteractHandEvent>(OnActivate);

    }

    private void OnActivate(EntityUid uid, MuleComponent component, InteractHandEvent args)
    {
        if(!TryComp<StrapComponent>(uid, out var strapComponent))
            return;

        if(strapComponent.BuckledEntities.Count <= 0)
            return;

        args.Handled = true;
    }
    private void OnStrapAttempt(EntityUid uid, MuleComponent component, StrapAttemptEvent args)
    {
        if (args.Strap.Comp.BuckledEntities.Count <= 0)
            args.Cancelled = true;
        if(!_tagSystem.HasTag(args.Buckle.Owner, "HideContextMenu"))
            _tagSystem.AddTag(args.Buckle.Owner, "HideContextMenu");
    }

    private void OnUnstrap(EntityUid uid, MuleComponent component, ref UnstrappedEvent args)
    {
        _physicsSystem.SetCanCollide(args.Buckle.Owner, true);
        if(_tagSystem.HasTag(args.Buckle.Owner, "HideContextMenu"))
            _tagSystem.RemoveTag(args.Buckle.Owner, "HideContextMenu");
    }

    // prevent the mule from fucking flying
    private void OnStrap(EntityUid uid, MuleComponent component, ref StrappedEvent args)
    {
        _physicsSystem.SetCanCollide(args.Buckle.Owner, false);
    }
}
