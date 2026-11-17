using Robust.Client.Graphics;
using Content.Goobstation.Shared.MartialArts.Components;

namespace Content.Goobstation.Client.MartialArts;

public sealed partial class MartialArtsComboOverlaySystem : EntitySystem
{
    [Dependency] private IOverlayManager _overlay = default!;

    [SubscribeLocalEvent]
    private void OnComboStartup(Entity<ComboPerformerComponent> ent, ref ComponentStartup args)
    {
        _overlay.AddOverlay(new MartialArtsComboOverlay());
    }

    [SubscribeLocalEvent]
    private void OnComboShutdown(Entity<ComboPerformerComponent> ent, ref ComponentShutdown args)
    {
        _overlay.RemoveOverlay<MartialArtsComboOverlay>();
    }
}
