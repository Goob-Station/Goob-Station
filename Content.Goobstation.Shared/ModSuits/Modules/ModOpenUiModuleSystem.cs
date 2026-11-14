using Content.Goobstation.Shared.ModSuits.Components;
using Content.Goobstation.Shared.ModSuits.Systems;
using Robust.Shared.Network;

namespace Content.Goobstation.Shared.ModSuits.Modules;

public sealed partial class ModOpenUiModuleSystem : EntitySystem
{
    [Dependency] private INetManager _net = default!;
    [Dependency] private ModSuitSystem _modSuit = default!;
    [Dependency] private SharedUserInterfaceSystem _ui = default!;

    [SubscribeLocalEvent]
    private void OnUsed(Entity<ModOpenUiModuleComponent> ent, ref ModModuleUsedEvent args)
    {
        if (!TryComp<ModSuitComponent>(args.Suit, out var suit))
            return;

        var target = ent.Comp.Slot is { } slot ? _modSuit.GetPart((args.Suit, suit), slot) : args.Suit;
        if (target is not { } uid || !_ui.HasUi(uid, ent.Comp.Key))
            return;

        _ui.OpenUi(uid, ent.Comp.Key, args.User, _net.IsClient);
        args.Handled = true;
    }
}
