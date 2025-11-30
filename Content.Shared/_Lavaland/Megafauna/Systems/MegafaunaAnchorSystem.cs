using Content.Shared._Lavaland.Megafauna.Components;
using Content.Shared._Lavaland.Megafauna.Events;

namespace Content.Shared._Lavaland.Megafauna.Systems;

public sealed class MegafaunaAnchorSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _xform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MegafaunaAnchorComponent, MapInitEvent>(OnComponentStartup);
        SubscribeLocalEvent<MegafaunaAnchorComponent, MegafaunaStartupEvent>(OnStartup);
        SubscribeLocalEvent<MegafaunaAnchorComponent, MegafaunaShutdownEvent>(OnShutdown);
    }

    private void OnComponentStartup(Entity<MegafaunaAnchorComponent> ent, ref MapInitEvent args)
    {
        _xform.AnchorEntity(ent.Owner);
        ent.Comp.Anchored = true;
    }

    private void OnStartup(Entity<MegafaunaAnchorComponent> ent, ref MegafaunaStartupEvent args)
    {
        _xform.Unanchor(ent.Owner);
        ent.Comp.Anchored = false;
    }

    private void OnShutdown(Entity<MegafaunaAnchorComponent> ent, ref MegafaunaShutdownEvent args)
    {
        _xform.AnchorEntity(ent.Owner);
        ent.Comp.Anchored = true;
    }
}
