using Content.Shared.SubFloor;
using Robust.Shared.Network;

namespace Content.Goobstation.Shared.ModSuits.Modules;

public sealed partial class ModTrayScannerModuleSystem : EntitySystem
{
    [Dependency] private INetManager _net = default!;
    [Dependency] private SharedEyeSystem _eye = default!;

    [SubscribeLocalEvent]
    private void OnActivated(Entity<ModTrayScannerModuleComponent> ent, ref ModModuleActivatedEvent args)
    {
        if (_net.IsClient || args.Wearer is not { } wearer || ent.Comp.User != null)
            return;

        ent.Comp.User = wearer;
        var user = EnsureComp<TrayScannerUserComponent>(wearer);
        user.Count++;
        if (user.Count == 1)
            _eye.RefreshVisibilityMask(wearer);
    }

    [SubscribeLocalEvent]
    private void OnDeactivated(Entity<ModTrayScannerModuleComponent> ent, ref ModModuleDeactivatedEvent args)
    {
        if (_net.IsClient || ent.Comp.User is not { } wearer)
            return;

        ent.Comp.User = null;
        if (!TryComp<TrayScannerUserComponent>(wearer, out var user))
            return;

        user.Count--;
        if (user.Count > 0)
            return;

        RemComp(wearer, user);
        _eye.RefreshVisibilityMask(wearer);
    }
}
