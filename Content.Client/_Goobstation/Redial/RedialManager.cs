using Content.Shared._Goobstation.Redial;
using Robust.Client;
using Robust.Shared.Network;

namespace Content.Client._Goobstation.Redial;

public sealed class RedialManager
{
    public void Initialize()
        => IoCManager.Resolve<IClientNetManager>().RegisterNetMessage<MsgRedial>(RedialOnMessage);

    private void RedialOnMessage(MsgRedial message)
        => IoCManager.Resolve<IGameController>().Redial(message.Address);
}
