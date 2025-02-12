using Content.Client.UserInterface.Fragments;
using Content.Shared._Goobstation.CartridgeLoader.Cartridges;
using Content.Shared.CartridgeLoader;
using Robust.Client.UserInterface;

namespace Content.Client._Goobstation.CartridgeLoader.Cartridges;

public sealed partial class MuleWranglerUi : UIFragment
{
    private MuleWranglerUiFragment? _fragment;

    public override Control GetUIFragmentRoot()
    {
        return _fragment!;
    }
    public override void Setup(BoundUserInterface userInterface, EntityUid? fragmentOwner)
    {
        _fragment = new MuleWranglerUiFragment();
        _fragment.OnMessageSent += (type, uid) =>
        {
            SendMuleWranglerMessage(type, uid, userInterface);
        };

    }

    public override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is not MuleWranglerUiState muleWranglerUiState)
            return;

        _fragment?.UpdateState(muleWranglerUiState);
    }

    public void SendMuleWranglerMessage(MuleWranglerMessageType type, NetEntity uid, BoundUserInterface userInterface)
    {
        var muleWranglerMessage = new MuleWranglerUiMessageEvent(type, uid);
        userInterface.SendMessage(new CartridgeUiMessage(muleWranglerMessage));
    }
}
