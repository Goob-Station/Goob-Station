// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared._DV.VendingMachines;
using Robust.Client.UserInterface;

namespace Content.Client._DV.VendingMachines.UI;

public sealed class ShopVendorBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private ShopVendorWindow? _window;

    public ShopVendorBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<ShopVendorWindow>();
        _window.SetEntity(Owner);
        _window.OpenCenteredLeft();
        _window.Title = EntMan.GetComponent<MetaDataComponent>(Owner).EntityName;
        _window.OnItemSelected += index => SendMessage(new ShopVendorPurchaseMessage(index));
    }
}