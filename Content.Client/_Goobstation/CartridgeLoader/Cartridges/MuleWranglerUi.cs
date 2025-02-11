using Content.Client._Goobstation.CatridgeLoader.Cartridges;
using Content.Client.UserInterface.Fragments;
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
    }

    public override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is not MuleWranglerUiState muleWranglerUiState)
            return;

        _fragment?.UpdateState(muleWranglerUiState);
    }
}
