// SPDX-FileCopyrightText: 2025 BeBright <98597725+be1bright@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 LuciferEOS <stepanteliatnik2022@gmail.com>
// SPDX-FileCopyrightText: 2025 LuciferMkshelter <154002422+LuciferEOS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 LuciferMkshelter <stepanteliatnik2022@gmail.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.NTR;

namespace Content.Goobstation.Client.NTR.UI;

public sealed class NtrTaskBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [ViewVariables]
    private NtrTaskMenu? _menu;

    protected override void Open()
    {
        base.Open();

        _menu = new NtrTaskMenu();

        _menu.OnClose += Close;
        _menu.OpenCentered();

        _menu.OnLabelButtonPressed += id =>
            SendMessage(new TaskPrintLabelMessage(id));

        _menu.OnSkipButtonPressed += id =>
            SendMessage(new TaskSkipMessage(id));
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
