// SPDX-FileCopyrightText: 2021 Watermelon914 <3052169-Watermelon914@users.noreply.gitlab.com>
// SPDX-FileCopyrightText: 2021 Watermelon914 <37270891+Watermelon914@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 Vordenburg <114301317+Vordenburg@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 ElectroJr <leonsfriedrich@gmail.com>
// SPDX-FileCopyrightText: 2024 Eoin Mcloughlin <helloworld@eoinrul.es>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 eoineoineoin <github@eoinrul.es>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2024 osjarw <62134478+osjarw@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Labels;
using Content.Shared.Labels.Components;
using Robust.Client.UserInterface;

namespace Content.Client.Labels.UI
{
    /// <summary>
    /// Initializes a <see cref="HandLabelerWindow"/> and updates it when new server messages are received.
    /// </summary>
    public sealed class HandLabelerBoundUserInterface : BoundUserInterface
    {
        [Dependency] private readonly IEntityManager _entManager = default!;

        [ViewVariables]
        private HandLabelerWindow? _window;

        public HandLabelerBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        {
            IoCManager.InjectDependencies(this);
        }

        protected override void Open()
        {
            base.Open();

            _window = this.CreateWindow<HandLabelerWindow>();

            if (_entManager.TryGetComponent(Owner, out HandLabelerComponent? labeler))
            {
                _window.SetMaxLabelLength(labeler!.MaxLabelChars);
            }

            _window.OnLabelChanged += OnLabelChanged;
            Reload();
        }

        private void OnLabelChanged(string newLabel)
        {
            // Focus moment
            if (_entManager.TryGetComponent(Owner, out HandLabelerComponent? labeler) &&
                labeler.AssignedLabel.Equals(newLabel))
                return;

            SendPredictedMessage(new HandLabelerLabelChangedMessage(newLabel));
        }

        public void Reload()
        {
            if (_window == null || !_entManager.TryGetComponent(Owner, out HandLabelerComponent? component))
                return;

            _window.SetCurrentLabel(component.AssignedLabel);
        }
    }
}