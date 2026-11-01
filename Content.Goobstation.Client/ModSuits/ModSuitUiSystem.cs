using Content.Goobstation.Shared.ModSuits;
using Content.Goobstation.Shared.ModSuits.Components;

namespace Content.Goobstation.Client.ModSuits;

public sealed partial class ModSuitUiSystem : EntitySystem
{
    [Dependency] private SharedUserInterfaceSystem _ui = default!;

    private readonly HashSet<EntityUid> _dirty = new();

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        if (_dirty.Count == 0)
            return;

        foreach (var suit in _dirty)
        {
            if (_ui.TryGetOpenUi(suit, ModSuitUiKey.Key, out var bui))
                bui.Update();
        }

        _dirty.Clear();
    }

    [SubscribeLocalEvent]
    private void OnSuitState(Entity<ModSuitComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        _dirty.Add(ent);
    }

    [SubscribeLocalEvent]
    private void OnSuitUpdated(Entity<ModSuitComponent> ent, ref ModSuitUpdatedEvent args)
    {
        _dirty.Add(ent);
    }

    [SubscribeLocalEvent]
    private void OnModuleState(Entity<ModModuleComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (ent.Comp.Suit is { } suit)
            _dirty.Add(suit);
    }
}
