// SPDX-FileCopyrightText: 2020 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2020 Peter Wedder <burneddi@gmail.com>
// SPDX-FileCopyrightText: 2020 Visne <vincefvanwijk@gmail.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Swept <sweptwastaken@protonmail.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Shared.Light.Components;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Timing;
using static Robust.Client.UserInterface.Controls.BoxContainer;

namespace Content.Client.Light.Components;

public sealed class HandheldLightStatus : Control
{
    private const float TimerCycle = 1;

    private readonly HandheldLightComponent _parent;
    private readonly PanelContainer[] _sections = new PanelContainer[HandheldLightComponent.StatusLevels - 1];

    private float _timer;

    private static readonly StyleBoxFlat StyleBoxLit = new()
    {
        BackgroundColor = Color.LimeGreen
    };

    private static readonly StyleBoxFlat StyleBoxUnlit = new()
    {
        BackgroundColor = Color.Black
    };

    public HandheldLightStatus(HandheldLightComponent parent)
    {
        _parent = parent;

        var wrapper = new BoxContainer
        {
            Orientation = LayoutOrientation.Horizontal,
            SeparationOverride = 4,
            HorizontalAlignment = HAlignment.Center
        };

        AddChild(wrapper);

        for (var i = 0; i < _sections.Length; i++)
        {
            var panel = new PanelContainer {MinSize = new Vector2(20, 20)};
            wrapper.AddChild(panel);
            _sections[i] = panel;
        }
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        _timer += args.DeltaSeconds;
        _timer %= TimerCycle;

        var level = _parent.Level;

        for (var i = 0; i < _sections.Length; i++)
        {
            if (i == 0)
            {
                if (level == 0 || level == null)
                {
                    _sections[0].PanelOverride = StyleBoxUnlit;
                }
                else if (level == 1)
                {
                    // Flash the last light.
                    _sections[0].PanelOverride = _timer > TimerCycle / 2 ? StyleBoxLit : StyleBoxUnlit;
                }
                else
                {
                    _sections[0].PanelOverride = StyleBoxLit;
                }

                continue;
            }

            _sections[i].PanelOverride = level >= i + 2 ? StyleBoxLit : StyleBoxUnlit;
        }
    }
}