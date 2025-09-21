using Content.Goobstation.Common.Cloning;
using Content.Shared.Heretic;
using Content.Shared.Store.Components;

namespace Content.Goobstation.Shared.Heretic;

// TODO put this in SharedHereticSystem if it ever gets moved out of _shitcode
public sealed class HereticCloningSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HereticComponent, TransferredToCloneEvent>(OnTransferredToClone);
    }

    private void OnTransferredToClone(Entity<HereticComponent> ent, ref TransferredToCloneEvent args)
    {
        RemComp<HereticComponent>(ent);
        RemComp<StoreComponent>(ent);
    }
}
