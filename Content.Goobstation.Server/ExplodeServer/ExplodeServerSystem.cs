using Content.Goobstation.Shared.ExplodeServer;

namespace Content.Goobstation.Server.ExplodeServer;

public sealed class ExplodeServerSystem : EntitySystem
{
    public void TriggerOverlay()
    {
        RaiseNetworkEvent(new ExplodeServerEvent(isExploding:true));
    }
}
