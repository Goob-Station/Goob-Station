using Content.Shared.Mining.Components;
using Content.Shared._NF.Mining.Components;

namespace Content.Shared.Mining;

public sealed partial class MiningScannerSystem : EntitySystem
{

    /// <inheritdoc/>
    public void NFInitialize()
    {
        SubscribeLocalEvent<InnateMiningScannerViewerComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(Entity<InnateMiningScannerViewerComponent> ent, ref ComponentStartup args)
    {
        if (!HasComp<MiningScannerViewerComponent>(ent))
        {
            SetupInnateMiningViewerComponent(ent);
        }
    }

    private void SetupInnateMiningViewerComponent(Entity<InnateMiningScannerViewerComponent> ent)
    {
        var comp = EnsureComp<MiningScannerViewerComponent>(ent);
        comp.ViewRange = ent.Comp.ViewRange;
        comp.PingDelay = ent.Comp.PingDelay;
        comp.PingSound = ent.Comp.PingSound;
        comp.QueueRemoval = false;
        comp.NextPingTime = _timing.CurTime + ent.Comp.PingDelay;
        Dirty(ent.Owner, comp);
    }
}
