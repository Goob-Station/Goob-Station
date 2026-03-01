// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Xenobiology.XenobiologyBountyConsole;
using Content.Shared.Cargo.Components;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Goobstation.Client.Xenobiology.XenobiologyBountyConsole;

[UsedImplicitly]
public sealed class XenobiologyBountyConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [ViewVariables]
    private XenobiologyBountyMenu? _menu;

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<XenobiologyBountyMenu>();

        _menu.OnFulfillButtonPressed += id =>
        {
            SendMessage(new BountyFulfillMessage(id));
        };

        _menu.OnSkipButtonPressed += id =>
        {
            SendMessage(new BountySkipMessage(id));
        };
    }

    protected override void UpdateState(BoundUserInterfaceState message)
    {
        base.UpdateState(message);

        if (message is not XenobiologyBountyConsoleState state)
            return;

        _menu?.UpdateEntries(state.Bounties, state.History, state.UntilNextSkip, state.UntilNextGlobalRefresh);
    }
}
