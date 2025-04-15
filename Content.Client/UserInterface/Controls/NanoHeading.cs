// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Client.Stylesheets;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.UserInterface.Controls
{
    public sealed class NanoHeading : Container
    {
        private readonly Label _label;
        private readonly PanelContainer _panel;

        public NanoHeading()
        {
            _panel = new PanelContainer
            {
                Children = {(_label = new Label
                {
                    StyleClasses = {StyleNano.StyleClassLabelHeading}
                })}
            };
            AddChild(_panel);

            HorizontalAlignment = HAlignment.Left;
        }

        public string? Text
        {
            get => _label.Text;
            set => _label.Text = value;
        }
    }
}