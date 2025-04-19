// SPDX-FileCopyrightText: 2025 BeBright <98597725+be1bright@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 LuciferMkshelter <stepanteliatnik2022@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Client.NTR;
using Content.Client.Cargo.UI;
using Content.Goobstation.Shared.NTR;
using Content.Shared.Cargo.Components;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Goobstation.Client.NTR.UI;

[UsedImplicitly]
public sealed class NtrTaskBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private NtrTaskMenu? _menu;

    public NtrTaskBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _menu = new();

        _menu.OnClose += Close;
        _menu.OpenCentered();

        _menu.OnLabelButtonPressed += id =>
        {
            SendMessage(new TaskPrintLabelMessage(id));
        };

        _menu.OnSkipButtonPressed += id =>
        {
            SendMessage(new TaskSkipMessage(id));
        };
    }
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        _menu?.Close();
    }

    protected override void UpdateState(BoundUserInterfaceState message)
    {
        base.UpdateState(message);

        if (message is not NtrTaskConsoleState state)
            return;

        _menu?.UpdateEntries(state.AvailableTasks, state.History, state.UntilNextSkip);
    }
}
