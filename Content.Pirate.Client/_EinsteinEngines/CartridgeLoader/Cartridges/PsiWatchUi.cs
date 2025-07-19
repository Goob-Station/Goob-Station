using Content.Client.UserInterface.Fragments;
using Content.Shared.CartridgeLoader;
using Content.Shared.CartridgeLoader.Cartridges;
using Robust.Client.UserInterface;

/// <summary>
/// ADAPTED FROM SECWATCH - DELTAV
/// </summary>

namespace Content.Pirate.Client._EinsteinEngines.CartridgeLoader.Cartridges;

public sealed partial class PsiWatchUi : UIFragment
{
    private PsiWatchUiFragment? _fragment;

    public override Control GetUIFragmentRoot()
    {
        return _fragment!;
    }

    public override void Setup(BoundUserInterface ui, EntityUid? owner)
    {
        _fragment = new PsiWatchUiFragment();
    }

    public override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is PsiWatchUiState cast)
            _fragment?.UpdateState(cast);
    }
}
