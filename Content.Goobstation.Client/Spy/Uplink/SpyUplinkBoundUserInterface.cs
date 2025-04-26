// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Shared.Spy;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Goobstation.Client.Spy.Uplink;

[UsedImplicitly]
internal sealed partial class SpyUplinkBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    private SpyUplinkMenu? _menu;
    private List<SpyBountyData> _bounties = [];
    private TimeSpan _refreshTime;

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<SpyUplinkMenu>();
        _menu.OnRefresh += RequestNewState;
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        switch (state)
        {
            case SpyUplinkUpdateState msg:
                _bounties = msg.Listings;
                _refreshTime = msg.Time;
                UpdateBounties();
                break;
        }
    }

    private void UpdateBounties()
    {
        _menu?.UpdateBounty(_bounties.ToList(), _refreshTime);
    }

    private void RequestNewState()
    {
        SendMessage(new SpyRequestUpdateInterfaceMessage());
        UpdateBounties();
    }
}
