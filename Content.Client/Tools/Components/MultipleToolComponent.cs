// SPDX-FileCopyrightText: 2020 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2020 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2020 zumorica <zddm@outlook.es>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1+git@googlemail.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Client.Message;
using Content.Client.Stylesheets;
using Content.Shared.Tools.Components;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Timing;

namespace Content.Client.Tools.Components
{
    public sealed class MultipleToolStatusControl : Control
    {
        private readonly MultipleToolComponent _parent;
        private readonly RichTextLabel _label;

        public MultipleToolStatusControl(MultipleToolComponent parent)
        {
            _parent = parent;
            _label = new RichTextLabel { StyleClasses = { StyleNano.StyleClassItemStatus } };
            _label.SetMarkup(_parent.StatusShowBehavior ? _parent.CurrentQualityName : string.Empty);
            AddChild(_label);
        }

        protected override void FrameUpdate(FrameEventArgs args)
        {
            base.FrameUpdate(args);

            if (_parent.UiUpdateNeeded)
            {
                _parent.UiUpdateNeeded = false;
                Update();
            }
        }

        public void Update()
        {
            _label.SetMarkup(_parent.StatusShowBehavior ? _parent.CurrentQualityName : string.Empty);
        }
    }
}