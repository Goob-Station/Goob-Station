// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Simon <63975668+Simyon264@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Research;
using Content.Shared.Research.Components;
using Robust.Client.UserInterface;

namespace Content.Client.Research.UI
{
    public sealed class DiskConsoleBoundUserInterface : BoundUserInterface
    {
        [ViewVariables]
        private DiskConsoleMenu? _menu;

        public DiskConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        {
        }

        protected override void Open()
        {
            base.Open();

            _menu = this.CreateWindow<DiskConsoleMenu>();

            _menu.OnServerButtonPressed += () =>
            {
                SendMessage(new ConsoleServerSelectionMessage());
            };
            _menu.OnPrintButtonPressed += () =>
            {
                SendMessage(new DiskConsolePrintDiskMessage());
            };
        }

        protected override void UpdateState(BoundUserInterfaceState state)
        {
            base.UpdateState(state);

            if (state is not DiskConsoleBoundUserInterfaceState msg)
                return;

            _menu?.Update(msg);
        }
    }
}