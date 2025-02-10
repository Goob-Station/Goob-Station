using Content.Shared._Goobstation.MULE.Components;
using Content.Shared.Buckle.Components;
using Content.Shared.Interaction;
using Content.Shared.Verbs;

namespace Content.Shared._Goobstation.MULE;

/// <summary>
/// This handles blocking interactions on buckles
/// </summary>
public sealed class BlockOnBuckleSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BlockOnBuckleComponent, ActivateInWorldEvent>(OnActivate);
    }
    private void OnActivate(EntityUid uid, BlockOnBuckleComponent component, ref ActivateInWorldEvent args)
    {
        if(!TryComp<BuckleComponent>(uid, out var buckleComponent))
            return;

        if(!buckleComponent.Buckled)
            return;

        args.Handled = true;
    }
}
