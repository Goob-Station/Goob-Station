// SPDX-FileCopyrightText: 2024 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Stacks;
using Content.Shared.Stacks;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Goobstation.Client.Stack
{
    [UsedImplicitly]
    public sealed class StackCustomSplitBoundUserInterface : BoundUserInterface
    {
        private IEntityManager _entManager;
        private EntityUid _owner;
        [ViewVariables]
        private StackCustomSplitWindow? _window;

        public StackCustomSplitBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        {
            _owner = owner;
            _entManager = IoCManager.Resolve<IEntityManager>();
        }

        protected override void Open()
        {
            base.Open();
            _window = this.CreateWindow<StackCustomSplitWindow>();

            if (_entManager.TryGetComponent<StackComponent>(_owner, out var comp))
                _window.SetMax(comp.Count);

            _window.ApplyButton.OnPressed += _ =>
            {
                if (int.TryParse((string?)_window.AmountLineEdit.Text, out var i))
                {
                    SendMessage(new StackCustomSplitAmountMessage(i));
                    _window.Close();
                }
            };
        }
    }
}