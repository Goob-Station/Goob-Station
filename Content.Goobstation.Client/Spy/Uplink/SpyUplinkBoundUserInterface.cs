using System.Linq;
using Content.Goobstation.Shared.Spy;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Goobstation.Client.Spy.Uplink;

[UsedImplicitly]
internal sealed partial class SpyUplinkBoundUserInterface : BoundUserInterface
{
    private SpyUplinkMenu? _menu;
    private List<SpyBountyData> _bounties = [];

    public SpyUplinkBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<SpyUplinkMenu>();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        switch (state)
        {
            case SpyUplinkUpdateState msg:
                _bounties = msg.Listings;

                UpdateBounties();
                break;
        }
    }

    private void UpdateBounties()
    {
        _menu?.UpdateBounty(_bounties.ToList());
    }
}
