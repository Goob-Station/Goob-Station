using Content.Goobstation.Shared.ModSuits.Modules;
using Robust.Client.Physics;
using Robust.Client.Player;

namespace Content.Goobstation.Client.ModSuits;

public sealed partial class ModMagneticHarnessPredictionSystem : EntitySystem
{
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private PhysicsSystem _physics = default!;

    [SubscribeLocalEvent]
    private void OnUpdateIsPredicted(Entity<ModMagnetizedGunComponent> ent, ref UpdateIsPredictedEvent args)
    {
        if (ent.Comp.LooseSince != null && ent.Comp.Wearer == _player.LocalEntity)
            args.IsPredicted = true;
    }

    [SubscribeLocalEvent]
    private void OnAfterState(Entity<ModMagnetizedGunComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        _physics.UpdateIsPredicted(ent);
    }
}
