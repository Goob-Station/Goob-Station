// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Goobstation.Wizard.Refund;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._Shitcode.Wizard.Refund;

[UsedImplicitly]
public sealed class StoreRefundBoundUserInterface : BoundUserInterface
{
    private StoreRefundWindow? _menu;

    public StoreRefundBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<StoreRefundWindow>();
        _menu.OpenCentered();
        _menu.ListingClicked += SendStoreRefundSystemMessage;
        _menu.RefundAllClicked += SendStoreRefundAllSystemMessage;

        _menu.Populate();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is not StoreRefundState refundState)
            return;

        _menu?.UpdateListings(refundState.Listings, refundState.RefundDisabled);
        _menu?.Populate();
    }

    public void SendStoreRefundAllSystemMessage()
    {
        SendMessage(new StoreRefundAllListingsMessage());
    }

    public void SendStoreRefundSystemMessage(NetEntity listingUid)
    {
        SendMessage(new StoreRefundListingMessage(listingUid));
    }
}